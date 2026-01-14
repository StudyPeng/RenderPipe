#ifndef NOISE_LIBRARY_INCLUDE
#define NOISE_LIBRARY_INCLUDE

// =======================
// HASH
// =======================

float Hash21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 78.233);
    return frac(p.x * p.y);
}

float2 Hash22(float2 p)
{
    p = float2(
        dot(p, float2(127.1, 311.7)),
        dot(p, float2(269.5, 183.3))
    );
    return frac(sin(p) * 43758.5453123);
}

// =======================
// White Noise
// =======================

float WhiteNoise(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
}

// =======================
// Simple Noise
// =======================

float SimpleNoise(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    f = f * f * (3.0 - 2.0 * f);

    float a = Hash21(i);
    float b = Hash21(i + float2(1, 0));
    float c = Hash21(i + float2(0, 1));
    float d = Hash21(i + float2(1, 1));

    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

// =======================
// Gradient Noise
// =======================

float2 Gradient(float2 p)
{
    float2 h = Hash22(p);
    float angle = h.x * 6.28318530718; // 2π
    return float2(cos(angle), sin(angle));
}

float GradientNoise(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);

    float2 u = f * f * (3.0 - 2.0 * f);

    float2 g00 = Gradient(i + float2(0, 0));
    float2 g10 = Gradient(i + float2(1, 0));
    float2 g01 = Gradient(i + float2(0, 1));
    float2 g11 = Gradient(i + float2(1, 1));

    float n00 = dot(g00, f - float2(0, 0));
    float n10 = dot(g10, f - float2(1, 0));
    float n01 = dot(g01, f - float2(0, 1));
    float n11 = dot(g11, f - float2(1, 1));

    float nx0 = lerp(n00, n10, u.x);
    float nx1 = lerp(n01, n11, u.x);

    return lerp(nx0, nx1, u.y) * 0.5 + 0.5;
}

// =======================
// Voronoi Noise
// =======================

float VoronoiNoise(float2 uv)
{
    float2 g = floor(uv);
    float2 f = frac(uv);

    float minDist = 1.0;

    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float2 lattice = float2(x, y);
            float2 h = Hash22(g + lattice);

            float2 diff = lattice + h - f;
            float dist = dot(diff, diff);

            minDist = min(minDist, dist);
        }
    }

    return sqrt(minDist);
}

#endif
