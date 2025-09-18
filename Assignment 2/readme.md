# Assignment 2

I used OpenTK library (I am on Mac). Once again, like assignment 1, I generated a Visual Studio sln file using the `dotnet` cli, so let me know if that is working. If not, I can try developing on a Windows.

I implemented all the operations that were required by the assignment. The vector addition, subtraction and matrix multiplication - I implemented with operator overloads. For the rotation matrix, I did rotate on the Z axis. You can see all the example values that I used in the Program.cs.

Example output:
```
===Vector Operations===
v1 = 1, 2, 3
v2 = 4, 5, 6
v1 + v2 = 5, 7, 9
v1 - v2 = -3, -3, -3
Dot product = 32
Cross product = -3, 6, -3

===Matrix Operations===
Scaling Matrix:
  2.00   0.00   0.00   0.00 
  0.00   2.00   0.00   0.00 
  0.00   0.00   2.00   0.00 
  0.00   0.00   0.00   0.00 

Rotation Matrix (Z-axis):
  0.71  -0.71   0.00   0.00 
  0.71   0.71   0.00   0.00 
  0.00   0.00   0.00   0.00 
  0.00   0.00   0.00   0.00 

Combined (Scale * Rotation):
  1.41  -1.41   0.00   0.00 
  1.41   1.41   0.00   0.00 
  0.00   0.00   0.00   0.00 
  0.00   0.00   0.00   0.00 

Original Vector: 1, 0, 0
Transformed Vector: 1.4142135, 1.4142135, 0
```