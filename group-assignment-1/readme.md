# Group Assignment One

### EBO Avoiding Repetition Discussion

We only need to define four vertices if we are using EBO. Without it, we need to define 6 vertices, and two pairs of those vertices are repeated. So, by removing these we are removing repetition.

example

No EBO
```cs
float[] vertices = { 
    // first triangle (bottom left triangle)
    -0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, // Bottom-left vertex
    -0.5f,  0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, // Top-left vertex
    0.5f, -0.5f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, // Bottom-right vertex

    // second triangle (top right triangle)
    0.5f, 0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, // Top-right vertex
    -0.5f,  0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, // Top-left vertex
    0.5f, -0.5f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f // Bottom-right vertex
};
```

With EBO

```cs
float[] vertices = {
    -0.5f,  0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, // Top-left vertex
    0.5f, 0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, // Top-right vertex
    0.5f, -0.5f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, // Bottom-right vertex
    -0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, // Bottom-left vertex
};
```

As you can see in the examples, the top left and bottom right vertices are repeated in the code snippet without EBO. And in the code snippet with EBO, that repetition is removed.

All of this was completed on Mac using `dotnet` cli. I did not generate sln files for the projects. Please let me know if you have trouble running them on Visual Studio. Also, for the last question, we were not 100% sure what method to use, so we had to use online resources and you can see that we applied the rotation matrix within the vertex shader glsl code directly. Please let us know if you would rather us do something different.