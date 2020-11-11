# ManagedSoftwareExecution (MSE)
## What the heck is that?
Fair question! MSE is an algorithm made for CosmosOS that can execute a machine code designed from the ground up from me.
By now, the MSE code only supports 8-bit iterators and registers, but will add support for 32-bit as well as 16-bit in future.
Actually, I don't think this project will be of any help right now, because a key component is missing: the compiler.
## How do I run code on Cosmos then?
Well, unless you want to learn how the machine code works and write a code in machine language yourself from scratch, you have to wait for the parser to be completed.
By now you can run the code I'll publish in test_resources and execute it in your own kernel with the help of this algorithm. The files you're mainly interested in are ***ManagedSoftwareExecution.cs*** and ***TasksManager.cs***.
Let's proceed in order:
### Include the namespace
just add the namespace **ManagedSoftwareImplementation.SoftwareExecution** in your code and you should be really good to go.
### Grab an executable
To be honest, there's no much choice, just go for ***test.o*** in **test_resources/**.
### Implement the needed syscalls
test.o needs two key system calls that you can create yourself actually. One is used to write to the screen the character contained into the register EAX(which is 8bit-only by now).
Take a look at ***Kernel.cs***, function **SysCallPrint** and **SysCallPrintLN**.
Once implemented these two functions in your kernel, you have to tell the executable to use them, but first we need the instance of the executable.
### Load the executable
To do that just create a variable with type **Software**, that I'll call *test_exec* for test purposes.
```
Software test_exec = new Software(resource_bytes);
```
in which resource_bytes is:
```
[ManifestResourceStream(ResourceName = "projectname.filepos")] static byte[] resource_bytes;
```
### Add the syscalls we implemented
To add the system calls that you implemented, just use the function ```test_exec.AddSysCall(name_of_the_function, syscall_number);```.
Please keep in mind that test.o requires the println system call to be 0 and normal print has to be 1.
### Run the code!
Now that we're all sorted, we just need to create an instance of the TaskManager that I will call testManager
```
TaskManager testManager = new TaskManager();
```
At this point we can load the code
```
testManager.Start(test_exec);
```
Now let's start the task manager!
```
testManager.StartExecution();
```
## Important notes
please note that:
- Once loaded the task manager, there's no way back to cosmos, if not handled by a system call
- I've spent a lot of time doing this and I'd appreciate a feedback of yours, about what you think must be changed and stuff like that, but please don't tell stuff like '32bit not supported' and stuff like that, because they'll come. Just be patient

That being said, enjoy your code!
