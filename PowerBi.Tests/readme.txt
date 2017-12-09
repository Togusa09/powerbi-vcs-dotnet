Firstly thanks a lot for using bddify; but I can see that you are using the old package: 
bddify is moved to GitHub and is renamed to BDDfy. The new nuget package name is 
TestStack.BDDfy. The samples have also all merged into one called TestStack.BDDfy.Samples.

So I just went ahead and changed your package to TestStack.BDDfy. If this is the first time you are 
downloading this package you will not have to do anything as you have now got the latest of the right 
package. If you are updating your existing bddify package, however, you will need to make some 
changes to your code so it will compile again:

 - Replace All (ctrl+shift+h) instances of '.Bddify(' with '.BDDfy('
 - Replace All instances of '.Bddify<' with '.BDDfy<' if you are using the overload with TStory type argument.
 - Replace All instances of 'using Bddify' with 'using TestStack.BDDfy'

That is it. You are now using the latest version of BDDfy. 

Sorry for the inconvenience.
 
To see the full details you may refer to my post about the change:
http://www.mehdi-khalili.com/bddify-moved-to-github-and-renamed-to-teststack-bddfy