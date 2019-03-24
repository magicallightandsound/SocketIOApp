# Magic Leap (Unity3D) Orcus-Barzoom socket.io Demo Project
Magic Leap project that demos a simple socket.io connection to https://acpt-barzoom.herokuapp.com 

The app demonstrates a simple shared 3D experience. 


The source files are: assets/main.cs, assets/ActsAsBarzoomable.cs, and assets/GameObjectFactory.cs file


Instructions (for two workstations, but you can test on more than two):

1) Load the Project into your Unity Editor on workstation #1
2) Load the Project into a Unity Editor on workstation #2
3) Press the Unity play button on workstation #1
4) Wait 5 seconds
5) Press the Unity play button on workstation #2
6) Wait until both Projects sync, when they sync you should see a blue cube in the game view of each Unity editor
7) Click on the Unity "scene" button in workstation #1, this should place the editor into scene mode
8) Click on the Unity "scene" button in workstation #2, this should place the editor into scene mode
9) On workstation #1, click on the blue cube and move it around. You should see the cube move in workstation #2.
10) On workstation #2, click on the blue cube and move it around. You should see the cube move in workstation #1.

Bug reports are welcome, send to rdegraci@gmail.com

Roadmap can be found at: https://www.pivotaltracker.com/n/projects/2321432

Requirements:
- Two or more workstations 
- Magic Leap SDK v0.19.0
- Unity 2018.1.9f1-MLTP10


#
Copyright 2018 Rodney Degracia

MIT License:

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
