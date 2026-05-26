# Custom MGFXC

This is an adapted version of the MGFXC from the version of MonoGame that Stardew Valley uses.

The changes include:
* A fix for symbol offsets in the constant buffers of compiled shaders. Might not be perfect but seems to work.
* A fix for the columns used by matrices in constant buffers, taken from MonoGame/MonoGame#8000
* Changes to allow using a much newer mojoshader version.

The former two fix weird exceptions when setting parameters.

The need for using a newer mojoshader is to fix a confusing issue with shaders unusing undeclared indentifiers.
The only case of this error online that I could find was in MonoGame/MonoGame#1021, which was opened in 2012 and
closed in 2016 with a message about how it will be circumvented by replacing MojoShader (which never happened).

Newer MonoGame uses a nuget package with their mojoshader version, which is how I found the change needed in
mojoshader itself to make it work here, but it is still largely out of date (though there appears to)

The changes can be found at:
* For MGFXC: https://github.com/spacechase0/ModdedStardewMonoGame/commits/modded-shaderfix-test
* For mojoshader: https://github.com/spacechase0/mojoshader/commits/stardew-monogame-fix/

> [!WARNING]
> The mgfxc above does not actually use the mojoshader above directly. The latter was built manually,
> with the resulting DLL copied directly into the resulting mgfxc.

> [!NOTE]
> Mojoshader was built with mostly default settings, except `BUILD_SHARED_LIBS` and `DEPTH_CLIPPING`
> were both turned on. At time of writing this was a debug build, using MinGW 13.0 w64
