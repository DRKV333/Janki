# Janki

This repo contains my Bachelor's thesis project. Janki is a flash card learning application modelled after [ANKI](https://github.com/ankitects/anki).

# About the Python scheduler

Earlier versions of the Janki client used the original Python scheduler in UWP through IronPython. The latest version does not have this any more, but you can see how it worked here: https://github.com/DRKV333/Janki/tree/onlab

This version of the project contains source code from the Python standard library, licensed under the [PSF License Agreement](https://github.com/DRKV333/Janki/blob/dc40cb8fc79870ffef1fc305df660aafe60b8f29/LibAnkiScheduler/lib/LICENSE).

The ANKI scheduler is available under the [GNU Affero General Public License](http://www.gnu.org/licenses/agpl.html), version 3 or later.

To comply with ANKI licensing requirements, the versions of Janki between and including commits [8ce67a0](https://github.com/DRKV333/Janki/commit/8ce67a00defa1cb981017fafea5a6fd5b2bb1545) to [d5388c7](https://github.com/DRKV333/Janki/commit/d5388c70ec85a2227bd351bc345e3ebc93b8cd37) are available under the [GNU Affero General Public License](http://www.gnu.org/licenses/agpl.html), version 3 or later. Other versions are provided under the MIT License. See the LICENSE file provided with the appropriate version of Janki for more information.

# How to compile

Clone the repository with submodules intact:
```
git clone --recursive https://github.com/DRKV333/Janki
```

The newer versions, with the C# based scheduler should build without any issues.

The most technically challenging part of this project was getting Python to run in UWP. I only managed to do this in Debug mode, it doesn't work in Release mode with AOT, but this could theoretically be fixed, by precompiling IronPython subclass types.

When switching to the old version of the code, remember to update submodules:
```
git checkout onlab
git submodule update --init --recursive
```

IronPython is a little bit weird, and my custom MSBuild automation is not perfect either, so for the first time, projects have to be build manually, in a specific order.

First, in **Release** mode, build these projects, in this order:
- IronPythonAnalyzer
- IronPythonCompiler

Then build these projects in **Debug** mode, in this order:
- IronPythonAnalyzer
- LibAnkiScheduler

The whole solution should now build in Debug mode without any problems. If you get errors relating to missing dependencies, try manually restoring:
```
dotnet restore
```