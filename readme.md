# ManagedSoftwareExecution
## What is it?
Well, MSE is a method that allows anybody to load external code from CosmosOS!
## How do i do that?
To get this working it's actually pretty simple:
### Install the package
Right click on your solution > Manage NuGET packages, then go to "Browse" and search ManagedSoftwareExecution, then install the package with the CosmosOS icon.
### Include the namespace
`using Cosmos.System.Emulation` should do the job ;)
### Choose an executable
#### a. Create your executable
To compile your code (by the moment ASM is only supported but I am working on C support too) go to the github page https://github.com/Nik300/ASMCompilerForMSE/releases and download the latest version available.
As of the moment of writing the current version is 1.0.0
#### b. Download a test program
If you don't know much about assembly and you don't want to waste much time on it go to [this link](https://github.com/Nik300/ASMCompilerForMSE/tree/main/Test%20programs) and download one of the test programs i compiled.
Note that these programs need to have special systemcalls (such as systemcall 0, 1, 2, 3, 4)
### Install the instruction set
By now the only available instruction set for MSE, is the one that I myself created: the FGMSEC Instruction Set. Don't ask me where that name came from because I don't know myself XD.
Anyhow to install it just do
`
FGMSECInstructionSet.Install();
`
in the 'BeforeRun' method in your code
### Run the executable
Now we just need to load the executable into an instance
```C#
FGMSECInstructionSet set = new FGMSECInstructionSet();
Executable exec;
exec = new Executable(resource_data, set, 3);
```
in which 'resource_data' is
```C#
[ManifestResourceStream(ResourceName = "project_name.path.to.executable.extension")] static byte[] resource_data;
```
now we simply load the executable data with `exec.ReadData();` and we run it by adding an infinite loop that executes its next instruction:
```C#
while (exec.running)
   exec.NextInstruction();
```
As you may notice, we are not in an actual 'endless' loop, but as soon as the program stops, it gives us back the control. This can be useful for the future (e.g. adding a TaskManager which is almos done, this means that MSE has multithreading :D)
## System Calls
Actually, running an executable as i did before, won't do much... this is because we didn't add any of the necessary Systemcalls that the executable requires. These are the basic systemcalls that ALL my test programs use:
```C#
exec.AddSystemCall((Executable caller) =>
{
  int addr = (int)((FGMSECInstructionSet)caller.usingInstructionSet).CPU.GetRegData(3);
  char c = (char)caller.Memory.ReadChar(addr);
  while (c != 0)
  {
    Console.Write(c);
    addr++;
    c = (char)caller.Memory.ReadChar(addr);
  }
});
exec.AddSystemCall((Executable caller) =>
{
  string input = Console.ReadLine();
  input += '\0';
  int addr = caller.Memory.Data.Count;
  int caddr = 0;
  for (int i = 0; i < input.Length; i++)
  {
    char c = input[i];
    if (!caller.Memory.AddChar(c))
    {
      caller.Memory.WriteChar(caddr, c);
      addr = 0;
      caddr++;
    }
  }
  ((FGMSECInstructionSet)caller.usingInstructionSet).CPU.SetRegData(3, (uint)addr);
});
exec.AddSystemCall((Executable caller) =>
{
  Console.Clear();
});
```
You can simply copy-paste those and run one of my test programs.
## Conclusion
MSE is a very powerful tool, and if you know how to use it, you can do very cool stuff with it!
I can't wait to see what kind of weird stuff you come up with it!
And PLEASE feedback everything!
If you're enjoying FEEDBACK or if you're hating it feedback it too giving reasons!
## Projects using MSE
If you didn't understand a word of what i said... well I can't blame you! Give a look at [my OS](https://github.com/Nik300/TestEnvOS) which uses MSE (look expecially at function 'ConstructExecutable' and the last part of 'RunCommand'

Enjoy your code!
