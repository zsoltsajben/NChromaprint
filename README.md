NChromaprint
============

NChromaprint is a university thesis project for porting the Chromaprint audio fingerprinting library (http://acoustid.org/chromaprint) to the .NET platform.

NOTES:
- The fingerprints created by NChromaprint still differ from the ones produced by Chromaprint (but are still usable audio fingerprints)
- The included Visual Studio solution contains other projects too, that use NChromaprint

The solution contains 4 projects:
- AudioCompare: UI for comparing audio files
- NChromaCompare: audio fingerprint comparing library
- NChromacompareTests: library for testing ChromaCompare -- the needed test audio files are absent from it
- NChromaprint: fingerprinting library
