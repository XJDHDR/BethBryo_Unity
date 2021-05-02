# BethBryo for Unity

BethBryo for Unity (BBU for short), is a project with the ultimate aim of getting the various RPGs made by Bethesda Game Studios running on the Unity Engine. That said, my plan is for game support to be added in a progressive manner, starting with TES4: Oblivion and adding the others after the previous game is pretty much fully supported.


- Game built from the ground up to use Unity's DOTS framework.
- Replicate original games as closely as possible.
- Extra features not found in the original games (e.g. first person body, co-op multiplayer, drawcall batching, etc.)
- Bring some features found in later games to earlier ones (e.g. occlusion planes to Oblivion and Morrowind)
- Support for existing games' modding frameworks as much as possible.


# What are the current short-term goals?

I am currently working on adding support for extracting files from Oblivion's BSAs. This work is almost finished at this time.

After that, I would like to add the following, probably simultaneously at times and not necessarily in this order:

- Full support for reading data from Oblivion's ESM and ESP files.
- Convert Oblivion's data files into Unity compatible formats:
	- **DDS**: Natively supported but some of Oblivion's textures have a bug where they report a mipmap count that differs from the actual number present.
	- **LOD and CMP**: Not required.
	- **NIF**: Needs to be converted to Unity compatible formats. Will likely use OBJs for static models and FBXs or COLs for animated/rigged ones.
	- **KF**: Likely need to be converted into Animation Frames or similar.
	- **EGM and TRI**: Will likely need to be converted to a text format that can be read by scripts that will deform the meshes as needed.
	- **SPT**: Unity supports SpeedTree models but not these ones. Likely need to be converted to a later version that is supported.
	- **MP3**: Natively supported.
	- **LIP**: Likely need to be converted into Animation Frames or similar.
	- **WAV**: Natively supported.
	- **FNT**: Natively supported?
	- **TEX**: Likely not natively supported.
	- **XML**: Natively supported in C#.
	- **SDP**: The PSO and VSO files contained within can be decompiled back into the original HLSL, which might be natively supported.
	- **BIK**: Will need to be converted into MP4 videos.
	- **ESM and ESP**: Useful data will likely be converted into XML format or similar that can be looked up more easily without parsing the whole file.
- Convert all data found in Oblivion.ESM and the game's DLC into Unity compatible formats (e.g. create a new Material for every texture, a new AudioClip for every sound, new Prefab for every record with a model, etc.)
- Modding support for the above.

After that, I will start implementing Oblivion's game mechanics. I won't let this schedule stop any contributors who would like to start adding things that are not in this roadmap.


# What are the current long-term goals?

I intend to eventually support every game created by Bethesda Game Studios that is based on the NetImmerse, GameBryo or Creation Engines. I intend on adding support in a step-wise manner; pretty much completing support for one game before moving on to the next.

The roadmap is as follows:

- Phase 1
	- TES4: Oblivion
- Phase 2
	- Fallout 3 and New Vegas
- Phase 3
	- TES3: Morrowind
- Phase 4
	- TES5: Skyrim
- Phase 5
	- TES5: Skyrim SE
- Phase 6
	- Fallout 4
- Phase 7
	- Possibly Starfield and/or TES6, depending on how similar their low-level data is to the previous games.

That said, if other contributors would like to start adding support for other games beforehand, it's unlikely I will say no.
