# Assignment One

I used OpenTK, not SharpDX - mainly because I want to use my Mac.

This was a good introduction to OpenTK and low level window rendering. Was happy that OpenTK is cross-platform and I could develop this on my Mac. Most of the work was just copied from the assignment instructions, but going through and finding out why each step was important / what each step is doing was very fun. Also learning that I need to create two triangles to form the rectangles as rectangles are not primitives was interesting.

To run on Mac
1. `cd Assignment\ 1`
2. `dotnet run`

For Windows - I used dotnet to add a sln file that can be opened with Visual Studio. From there it can be run. I haven't been able to test it on Windows - so if it doesn't work, I believe `dotnet run` in the `rectangle` folder should also give the same result on Windows.