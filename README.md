Algorithmia
===========

Algorithm and data-structure library for .NET 3.5 and up. Algorithmia contains sophisticated algorithms and data-structures like graphs, priority queues, command, undo-redo and more. 

Algorithmia is one of the pillars of [LLBLGen Pro v3 and up](http://www.llblgen.com/) and is used in production successfully since May 2010. Many of the classes and algorithms in Algorithmia contain references to papers and articles on Wikipedia or other websites where you can find additional information regarding the algorithm or data-structure implemented. 

The core design of Algorithmia is about making algorithms and data-structures which are known for many years available to the .NET developer in easy to use and properly documented classes and methods. No class or method implemented in Algorithmia re-implements a .NET BCL (base class library) class or method unless it was necessary to do so (e.g. the linked list in .NET is re-implemented as it doesn't have an O(1) concat operation, which is necessary for the Fibonacci heap). 

Although Algorithmia is stable since May 2010, we still update it with new algorithms and data-structures. If you have algorithms or data-structures to contribute, please create a fork and send us a pull request! :)

Algorithmia v1.2 is in development on the master branch, and is the foundation for LLBLGen Pro v5. To obtain the v1.1 source, used in LLGLGen Pro v4 and lower, please see the v1.1 branch/tag. 

### Getting a binary version via NuGet

Algorithmia is also [available on Nuget](https://nuget.org/packages/SD.Tools.Algorithmia/). When you obtained Algorithmia via Nuget, it's recommended you check out the unit tests in this source repository and/or look at the SD.Tools.Algorithmia.chm reference manual included in the source repository, which is generated from the sourcecode.

### License
Algorithmia is &copy; 2010-2014 [Solutions Design bv](http://www.sd.nl/) and is released under the [BSD2 license](https://github.com/SolutionsDesign/Algorithmia/blob/master/LICENSE.txt).
