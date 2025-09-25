# Assignment 3

I used the OpenTK library once again. The cube is rendered with the EBO method. I plotted out all of the cube vertices and then the indices. I used projection * view * model in the shader. On render frame I am recalculating the model based on the x and y rotation values which are updated with keydown checks.