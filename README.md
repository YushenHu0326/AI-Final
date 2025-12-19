# Introduction
This is the code based for my implementation of the final project in Intro to Artificial Intelligence I.

- Software used: Unity Editor version 2022.3.48f1. Please go to the Unity official website to download the Unity Hub in order to run this project.
- Language used: C#, Unity ShaderLab (for parallel GPU computation)

# Description
**Please be aware that this description assumes you have a basic knowledge of Unity Game Programming and Developing skills. If not, please ask online or Generative AI for explanations for the details you don't understand.**

Two path-planning algorithms: A* and RTT, are implemented.
A* is implemented in 3D space, based on the NavMesh.cs which defines the 3D space. It uses the classic implementation with a few tweaks:
- Neighboring cells only count for the adjacent cells, but not diagonal cells
- Whether a cell is blocked is judged by running a box check at each cell, even a slightest overlap with a solid object will mark the cell as "blocked"

Hence, to get better routing performance, increase the cell numbers by increasing **cellX, cellY, cellZ** under NavMesh.cs respectively. However, it will significantly increase the computation time.

RTT is a customized version implemented after reading the paper and watch a couple of online video tutorials. It runs in a while loop conditioned by that: the current node selected will have a direct path (SphereCast returns false, meaning no blockage among the direct route) to the destination. If not, a random point which the current node will have a clear path to is sampled, and that random point is then defined to be the current node.

I bring my previous project of [Marching Tetrahedra](https://github.com/YushenHu0326/3DModeling-Unity) to use. Any grid cell which the value is larger than the surface value will be marked as "blocked". Notice that the marching tetrahedra grid size must be the same as the NavMesh.

Also, for the simplicity, I refered the marching tetrahedra method as marching cube in my paper.

**Two external open sourced scripts are used. They are: [Keijiro's implementation of 3D Perlin Noise](https://github.com/keijiro/PerlinNoise), and [Ashley's implementation](https://gist.github.com/ashleydavis/f025c03a9221bc840a2b) of a free camera. The rest of the codes are all written by myself.**

# Live Demo
You can play the demo [here](https://play.unity.com/en/games/4563cac1-2d14-4826-ae63-7cc15b40d4c7/ai-final-demo). Please be noted, as in the paper and in the description of the game page, that the noise scene doesn't work.

# GIF Demo
To demonstrate how the tree is expanded:
![demo](https://github.com/user-attachments/assets/780d6093-8f84-4ab3-b8f4-afa796e92f1c)

