<p align="center">

  <img src="Resources/lol2gltf-logo.png"> 

  <h1 align="center">lol2gltf</h1>

  <p align="center">
    <a href="https://github.com/Crauzer/lol2gltf/releases">
      <img alt="GitHub release (latest by date)" src="https://img.shields.io/github/v/release/Crauzer/lol2gltf?color=teal&label=Download&logo=github&style=for-the-badge">
    </a>
  <a href="https://github.com/Crauzer/lol2gltf/releases">
      <img alt="GitHub All Releases" src="https://img.shields.io/github/downloads/Crauzer/lol2gltf/total?style=for-the-badge">
    </a>
  </p>

  <p align="center">
    Tool for converting between the glTF format and League of Legends models and animations
  </p>
</p>

<p align="center">
  <a href="https://github.com/Crauzer/lol2gltf/wiki">
      <h1 align="center"> How to use </h1>
  </a>
</p>

* Features:
* * SKN -> glTF
* * SKN + SKL -> glTF
* * SKN + SKL + Animations -> glTF
* * Mapgeo -> glTF
* * Texture bundling (SKN)

* Upcoming features:
* * Non-cmd Application with a good looking UI
* * Converting glTF back to SKN, SKL and ANM

* Known Issues:
* * Version 1.0 has a bug where textures will not be exported to glTF if you also don't supply animations
* * Version 1.0 exports the model flipped on the X axis

<h1 align="center"> Preview </h1>

<p align="center">
  <a href="https://thumbs.gfycat.com/HappyRectangularAntelopegroundsquirrel-size_restricted.gif">
    <img src="https://thumbs.gfycat.com/HappyRectangularAntelopegroundsquirrel-size_restricted.gif"></>
  </a>
  <a href="https://thumbs.gfycat.com/ShorttermThoroughDingo-size_restricted.gif">
    <img src="https://thumbs.gfycat.com/ShorttermThoroughDingo-size_restricted.gif"></>
  </a>
</p>

# Special thanks to:
[vpenades](https://github.com/vpenades) - for creating the [SharpGLTF library](https://github.com/vpenades/SharpGLTF) and fixing a critical bug before release

[moonshadow565](https://github.com/moonshadow565) - for helping me reverse engineer the ANM file format
