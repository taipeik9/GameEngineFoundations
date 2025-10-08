# Assignment 5

So, this is assignment 5, implementing a Phong lighting system within OpenTK.

Honestly, this assignment felt pretty easy, especially seeing that we were given the shader code in advance. It was a bit strange transitioning to using shaders as external files but still pretty simple. I just moved some IO code into my "CompileShader" function and it takes a path instead of a string source code now.

I did, however, change the shader code a bit. I hope that is alright. I wanted to use the simple vertices and indices that I'd already set up, and all of the solutions that involved passing the normals into the fragment shader did not allow for this. That is, until I came across an article (https://www.enkisoftware.com/devlogpost-20150131-1-Normal-generation-in-the-pixel-shader). In this, the author explains how they calculate the normals within the fragment shader and provided a code example. So, I modified the given shader files to do that instead of needing to pass in a bunch of normals in the vertex array, and also avoided having to modify it to be more complex.

I didn't really face any trouble coding this, it was pretty straight forward. I just looked at which new uniform variables needed to be added from the C# code to the shaders and then did so. For the lighting-related ones I passed them in OnLoad because they're unchanging. As for the camera position, they are passed in the OnRenderFrame, as the camera position changes based on the user's controls.

The strange controls are because I didn't want to implement camera rotation... haha. So, the WASD keys move the camera and the arrow keys rotate the **cube**, not the camera.

Instructions:

1. Ensure you have the dotnet framework installed
2. `cd` to the Cube folder
3. `dotnet run`