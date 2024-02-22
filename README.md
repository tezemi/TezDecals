# TezDecals

Simple and lightweight Unity library that adds a component for creating decals. This project is based off of [Deni35's Simple Decal System](https://assetstore.unity.com/packages/tools/particles-effects/simple-decal-system-13889), but updated to work with newer versions of Unity, with additional features such as runtime support and a few fixes. Compatible with ProBuilder.

With the Decal component, you can quickly add decals to your Unity project. 

![image](https://github.com/tezemi/TezDecals/assets/59236027/a18195a4-58e3-4586-936b-849122c4f05d)
![image](https://github.com/tezemi/TezDecals/assets/59236027/c0b0a17f-a2cb-46ba-b090-53408777d3ee)

# Installation
The easiest way to install TezDecals is to install it as a package.
1. Window -> Package Manager
2. Click the "+" button.
3. Select "Add package from git URL."
4. Enter the following URL: `git@github.com:tezemi/TezDecals.git`
5. Click "Add."
   
TezDecals has no additional dependencies. Only compatible with Unity 2020.3 and forward. Mostly tested in 2022.3.

# Getting Started
To create a decal, create a new game object, and add the "Decal" component to it.

![image](https://github.com/tezemi/TezDecals/assets/59236027/81405ba8-2984-49c9-9199-2e3761960260)

When you first create a new decal, you will notice a blue arrow pointing in the forward direction. Decals always need to be perpendicular from the surface they will appear on. In other words, the arrow should be pointing away from where your decal will be.

![image](https://github.com/tezemi/TezDecals/assets/59236027/5bdc4c6b-318b-48f9-969c-2afb007be7d7)

This is what a newly created decal component looks like:

![image](https://github.com/tezemi/TezDecals/assets/59236027/5492d4f4-0d8e-4f98-9170-c3213a3a9420)

Here is a quick explanation of each property:
- **Fixed Aspect**: True by default, this will cause the decal's scale to stay at a fixed ratio that matches the shape of the sprite. This is good for ensuring your decal won't stretch or warp when resizing it, however, sometimes that effect can be desirable, so this can be disabled.
- **Layer Mask**: Defines the layers that the decal will appear on. Can be useful for getting the decal to appear on some surfaces, while avoiding others (for example, water).
- **Max Angle**: The max angle at which a decal will "wrap" around a surface. More will be explained later.
- **Offset**: The distance the decal will be generated from the surface. This prevents z-clipping issues and can be increased if needed.
  
When it comes to actually selecting your decal, you will need to create a material and sprite sheet.

## Creating the Sprite Sheet 
1. First, import the texture that contains your decals.
2. Change the texture type to Sprite.
3. Change the sprite mode to Multiple.
4. Open the Sprite Editor.

![image](https://github.com/tezemi/TezDecals/assets/59236027/bb38f1ce-0441-414b-83ad-5254f1dd90b2)

6. Slice the sprites however needed, then click Apply.
   
![image](https://github.com/tezemi/TezDecals/assets/59236027/878f550b-cab1-436a-8523-0485a33f9958)

## Creating the Material
1. Create a new Material.
2. You can use whatever shader you want, as long as it supports cutout rendering. For this example, we will use the standard shader.
3. Change the Rendering Mode to Cutout.
4. Assign the main texture to be your sprite sheet.
5. You may need to change the Alpha Cutoff depending on whether or not you did a good job in Photoshop 😉

![image](https://github.com/tezemi/TezDecals/assets/59236027/b485b946-8f35-40f4-b207-2c800737a855)

## Almost Done
Now that you have a texture and material, go assign the material in your decal. This will cause the sprite picker to appear.

![image](https://github.com/tezemi/TezDecals/assets/59236027/8b91dadc-f657-4304-872b-887b4f2bf9ab)

You may now select which sprite from the sheet you want to use. Move the decal into position on the surface it will appear, it should update automatically.

![GIF](https://github.com/tezemi/TezDecals/assets/59236027/604d7541-17d5-4d27-8752-60eefb3f3c6e)

If a decal doesn't update, or you move the surface under it, you can click the "Generate" button to regenerate the decal without having to move it again. The decal will appear on any surface that is inside the bounding box.
## A Few Notes
- Changing the decal's X or Y scale will increase or decrease its size.
- Changing the decal's Z scale will increase the size of its bounding box.
- You can use the Max Angle property to prevent the decal from appearing on the edges of surfaces. In the below example, the Max Angle went from 90 to 75.

![image](https://github.com/tezemi/TezDecals/assets/59236027/e56e0ce3-94c2-4a55-b3b4-08b5d3e382b2)
![image](https://github.com/tezemi/TezDecals/assets/59236027/80788269-0a84-4dfe-a5c0-906b35757e5d)

- Decals always face away from their surface. In order to have a decal appear on the corner of something, you must rotate it accordingly.

![image](https://github.com/tezemi/TezDecals/assets/59236027/e5260500-5ee6-4758-93fa-c5e2f1ea5a58)

- If a decal doesn't move or need to be regenerated throughout your scene, you may want to mark it as static.

# Creating Decals at Runtime (In Code)
Creating decals at runtime is fairly straightforward. You may use the method `Decal.CreateDecal(Vector3, Quaternion, Material, Sprite)` to create a new decal. This will create a new game object with the decal already attached to it.
It takes the following arguments:
- The coordinates where to create the decal.
- The rotation of the decal.
- The material to use.
- The sprite to use.
- Optional: The max angle.
- Optional: The offset.

Using this method, it's fairly easy to create a decal based off of a raycast. This can be used to create things like bullet holes and blood splatters, or other less violent things if you want. Here's an example:
```
var transform = Camera.current.transform;
var raycastHit = Physics.Raycast(new Ray(transform.position, transform.forward), out var hitInfo);

if (raycastHit)
{
	Decal.CreateDecal
	(
		hitInfo.point,
		Quaternion.FromToRotation(Vector3.zero, -hitInfo.normal),
		DecalMaterial,
		DecalSprite
	);
}
```
This would create a decal in front of the camera, facing away from the hit surface. It will be generated right after it's created.
 
 
 
