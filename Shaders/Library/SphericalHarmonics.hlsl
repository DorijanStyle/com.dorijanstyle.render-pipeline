#pragma once

struct SphericalHarmonicsL2
{
    half4 a0;
    half4 a1;
    half a2;
};

struct SphericalHarmonicsL2RGB
{
    SphericalHarmonicsL2 r;
    SphericalHarmonicsL2 g;
    SphericalHarmonicsL2 b;
};

SphericalHarmonicsL2RGB MulSH3(in SphericalHarmonicsL2 sh, in half3 color)
{
    SphericalHarmonicsL2RGB result;
    result.r.a0 = sh.a0 * color.r;
    result.r.a1 = sh.a1 * color.r;
    result.r.a2 = sh.a2 * color.r;
    result.g.a0 = sh.a0 * color.g;
    result.g.a1 = sh.a1 * color.g;
    result.g.a2 = sh.a2 * color.g;
    result.b.a0 = sh.a0 * color.b;
    result.b.a1 = sh.a1 * color.b;
    result.b.a2 = sh.a2 * color.b;
    return result;
}

SphericalHarmonicsL2 AddSH3(in SphericalHarmonicsL2 a, in SphericalHarmonicsL2 b)
{
    SphericalHarmonicsL2 result;
    result.a0 = a.a0 + b.a0;
    result.a1 = a.a1 + b.a1;
    result.a2 = a.a2 + b.a2;
    return result;
}

SphericalHarmonicsL2RGB AddSH3(in SphericalHarmonicsL2RGB a, in SphericalHarmonicsL2RGB b)
{
    SphericalHarmonicsL2RGB result;
    result.r = AddSH3(a.r, b.r);
    result.g = AddSH3(a.g, b.g);
    result.b = AddSH3(a.b, b.b);
    return result;
}

SphericalHarmonicsL2 SHBasisFunction3(in float3 direction)
{
    SphericalHarmonicsL2 result;
    result.a0[0] = 0.282095f;                               // L = 0
    result.a0[1] = -0.488603f * direction.y;                 // L = 1
    result.a0[2] = 0.488603f * direction.z;
    result.a0[3] = -0.488603f * direction.x;
    result.a1[0] = 1.092548f * direction.x * direction.y;   // L = 3
    result.a1[1] = -1.092548f * direction.y * direction.z;
    result.a1[2] = 0.315392f * (3 * direction.z * direction.z - 1.0f);
    result.a1[3] = -1.092548f * direction.x * direction.z;
    result.a2 = 0.546274f * (direction.x * direction.x - direction.y * direction.y);
    return result;
}