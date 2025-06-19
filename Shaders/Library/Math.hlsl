#pragma once

#define PI          3.14159265359
#define HALF_PI     1.570796327


#define GenerateSqr(T) T sqr(T x) { return x * x; }
GenerateSqr(half);
GenerateSqr(float);
GenerateSqr(double);

float luminance(float3 color)
{
    return dot(color, float3(0.2126, 0.7152, 0.0722));
}