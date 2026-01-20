To build shaders:
```
# Only needed the first time - we need the old version for SDV specifically
dotnet tool install -g dotnet-mgfxc --version 3.8.0.1641 --allow-downgrade

# Then, from inside `assets_work/`
mgfxc GenericModelEffect.fx ../assets/GenericModelEffect.mgfxo /Profile:OpenGL
```
