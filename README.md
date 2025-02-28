<p align="center">
  <img src="Resources/lol2gltf-logo.png" alt="lol2gltf logo"> 
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
    A powerful tool for converting between the glTF format and League of Legends models and animations
  </p>
</p>

## 📋 Overview

lol2gltf is a command-line utility that enables seamless conversion between League of Legends' proprietary 3D formats and the industry-standard glTF format. This tool is essential for game modders, artists, and developers working with League of Legends assets.

## ✨ Features

- **Skinned Mesh Conversion**: Convert League's SKN/SKL/ANM files to glTF and back
- **Map Geometry Conversion**: Transform League's map geometry files to glTF
- **Static Mesh Conversion**: Convert glTF to League's static mesh formats (SCB/SCO)
- **Animation Support**: Include League animations in your glTF exports
- **Texture Integration**: Bundle textures with your models

## 🚀 Getting Started

### Installation

1. Download the latest release from the [Releases page](https://github.com/Crauzer/lol2gltf/releases)
2. Extract the ZIP file to a location of your choice
3. Run the tool from the command line as described below

## 📖 Command Reference

lol2gltf offers several conversion modes, each with its own set of parameters:

### Converting Skinned Mesh to glTF (`skn2gltf`)

Converts League's skinned mesh files (SKN, SKL, ANM) into a glTF asset.

```
lol2gltf skn2gltf -m <skn_path> -s <skl_path> -g <gltf_path> [-a <anm_folder>] [--materials <material_names>] [--textures <texture_paths>]
```

**Required Parameters:**
- `-m, --skn`: Path to the Simple Skin (.skn) file
- `-s, --skl`: Path to the Skeleton (.skl) file
- `-g, --gltf`: Path for the output glTF file (use .glb extension for binary format)

**Optional Parameters:**
- `-a, --anm`: Path to a folder containing Animation (.anm) files
- `--materials`: Material names for textures (must match the count of texture paths)
- `--textures`: Paths to texture files (must match the count of material names)

**Example:**
```
lol2gltf skn2gltf -m Aatrox.skn -s Aatrox.skl -g Aatrox.glb -a animations/
```

### Converting glTF to Skinned Mesh (`gltf2skn`)

Converts a glTF asset into League's skinned mesh formats (SKN, SKL).

```
lol2gltf gltf2skn -g <gltf_path> -m <skn_path> [-s <skl_path>]
```

**Required Parameters:**
- `-g, --gltf`: Path to the glTF asset (.glb or .gltf)
- `-m, --skn`: Path for the output Simple Skin (.skn) file

**Optional Parameters:**
- `-s, --skl`: Path for the output Skeleton (.skl) file (if not specified, will use the same name as the SKN file)

**Example:**
```
lol2gltf gltf2skn -g Aatrox.glb -m Aatrox.skn
```

### Converting Map Geometry to glTF (`mapgeo2gltf`)

Converts League's map geometry files into a glTF asset.

```
lol2gltf mapgeo2gltf -m <mapgeo_path> -b <matbin_path> -g <gltf_path> [-d <gamedata_path>] [-x <flip_x>] [-l <layer_policy>] [-q <texture_quality>]
```

**Required Parameters:**
- `-m, --mgeo`: Path to the Map Geometry (.mapgeo) file
- `-b, --matbin`: Path to the Materials Bin (.materials.bin) file
- `-g, --gltf`: Path for the output glTF file (use .glb extension for binary format)

**Optional Parameters:**
- `-d, --gamedata`: Path to the Game Data directory (required for bundling textures)
  - In order to correctly bundle all textures into the glTF file, you should make sure to export all relevant `.wad.client` files into this folder (MapXX.wad.client, some maps require the DATA wads as well).
- `-x, --flipX`: Whether to flip the map node's X axis (default: true)
- `-l, --layerGroupingPolicy`: Layer grouping policy for meshes (Default, Ignore)
- `-q, --textureQuality`: Quality of textures to bundle (Low = 4x, Medium = 2x)

### Converting glTF to Static Mesh (`gltf2static`)

Converts a glTF asset into League's static mesh formats (SCB/SCO).

```
lol2gltf gltf2static -i <gltf_path> -o <output_path>
```

**Required Parameters:**
- `-i, --input`: Path to the input glTF file
- `-o, --output`: Path to the output SCB/SCO file (extension determines format)

**Example:**
```
lol2gltf gltf2static -i prop_rock.glb -o prop_rock.scb
```

## 📚 Additional Resources

- [Video Tutorial (Outdated)](https://www.youtube.com/watch?v=XxSGk6SAcAM): Comprehensive guide to using lol2gltf

## 🖼️ Gallery

<p align="center">
  <a href="https://i.imgur.com/Gu31ztz.jpg">
    <img src="https://i.imgur.com/Gu31ztz.jpg" width="45%">
  </a>
  <a href="https://i.imgur.com/NfBlga6.jpg">
    <img src="https://i.imgur.com/NfBlga6.jpg" width="45%">
  </a>
  <a href="https://i.imgur.com/psWiYa2.jpg">
    <img src="https://i.imgur.com/psWiYa2.jpg" width="45%">
  </a>
  <a href="https://i.imgur.com/jx8yuHI.jpg">
    <img src="https://i.imgur.com/jx8yuHI.jpg" width="45%">
  </a>
  <a href="https://thumbs.gfycat.com/HappyRectangularAntelopegroundsquirrel-size_restricted.gif">
    <img src="https://thumbs.gfycat.com/HappyRectangularAntelopegroundsquirrel-size_restricted.gif" width="45%">
  </a>
  <a href="https://thumbs.gfycat.com/ShorttermThoroughDingo-size_restricted.gif">
    <img src="https://thumbs.gfycat.com/ShorttermThoroughDingo-size_restricted.gif" width="45%">
  </a>
</p>

## 📜 License

This project is licensed under the terms found in the [LICENSE](LICENSE) file.

## 🔮 Upcoming Features

- glTF to ANM conversion
- Improved texture handling
- Support for more League file formats

---

<p align="center">
  <img src="https://www.khronos.org/assets/uploads/apis/2017-collada-gltf-positioning.png" alt="glTF positioning">
</p>
