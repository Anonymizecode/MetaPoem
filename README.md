# MetaPoem: Model Library, Unity Loader and Licensing Notice

Here contains:

- The MetaPoem model dataset (<https://pan.baidu.com/s/1ADSr8zG4dlaNe2tQxbWf8w?pwd=1111>), provided as Unity packages (`.unitypackage` files). The full dataset comprises about 4,000 3D models of literary imagery in about 350 categories, but the third-party models among them are not included (see Licensing below).
- `unity/`: Unity loading code (MIT License) and a sample manifest. Given an imagery name such as `moon`, the loader places the corresponding model in the scene (see Usage below).
- `sources/categories.csv`: the category index (number and Chinese name of each category).
- This README, which describes the licensing status of the models.

## Licensing

Some of the models are from third-party assets represented by the Unity Asset Store. They remain the property of their original authors and are subject to their publishers' licenses, and the standard Asset Store EULA does not permit asset files to be redistributed on their own. They are therefore not included in this link and must be obtained from their original sources.

The categories with the following numbers include the third-party models (the specific category of the corresponding number details in ` sources/categories. csv `): 1, 3, 7, 9, 11, 17, 20, 28, 31-32, 34, 38, 43, 46, 52, 55, 58-61, 63-65, 74-77, 83-89, 91, 102-103, 105, 141-144, 147-149, 151, 164-165, 177, 181-184, 224-225, 227, 233, 235, 240, 242, 253, 255, 257, 259, 261-262, 264, 269, 271, 283, 291, 301-333, 336-350. Accordingly, these categories are incomplete, or entirely absent, in this link.

## Usage

The loader contains no models; it uses the models that you import into your own Unity project. To reproduce the scenes shown in the paper you therefore also need the third-party models, which must be obtained from their original sources.

Requirements: Unity 2021.3 or later.

1. Copy `unity/` into your project's `Assets/` folder.
2. Import the models you have obtained, from this link and from the original sources of the third-party models.
3. Write a manifest modeled on `unity/example_manifest.json` and fill in the GUID of each prefab (the value after `guid:` in the prefab's `.meta` file).
4. Add `ImageryModelLoader` to a GameObject, assign the manifest and call `loader.Spawn("moon")`. See `LoadImageryExample.cs`.

An imagery entry can be addressed by its Chinese name, English name or numeric id. Missing models are skipped and reported through the `MissingImagery` event. The loader only searches the local project.

## Rights holders

If you are a rights holder, please contact richenliu@ruc.edu.cn and we will promptly remove the relevant content. Everything in this link is provided "as is", and this README is not legal advice.
