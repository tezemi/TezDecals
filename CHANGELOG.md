# Change Log:

## 1.1.0
- Added a dropdown for selecting the decal source.
- Significantly improved performance of decals by making use of readonly mesh data and automatically excluding submeshes that don't intersect with the decal.
- Added a new static decal creation method that allows you to specify a specific decal source.

## 1.0.10
- Decals now generate using triangles from intersecting submeshindex, improving performance for combined meshes.

## 1.0.9
- Decals no longer try to generate using unreadable meshes.

## 1.0.8
- Removed debug messages.

## 1.0.7
- Added an additional methods for creating decals that let you specify mesh collider generation.

## 1.0.6
- Added an option to generate decals when they are intersecting with mesh colliders (as opposed to just renderers).

## 1.0.5
- Bug fix: Some static decal creation methods weren't properly setting the layer mask.

## 1.0.4
- Added additional methods for creating decals which let you specify a parent.
- Decals now generate on both MeshRenderers and SkinnedMeshRenderers.

## 1.0.3

- Added additional methods for creating decals which incorporate scale.
- Bug fix: Fixed issues with the max angle property.

## 1.0.2

- Bug fix: Added the meta file for the changelog so it would stop creating an error.

## 1.0.1

- Bug fix: Fixed the max angle not working properly against surfaces that have a rotation.

## 1.0.0

- Initial release
