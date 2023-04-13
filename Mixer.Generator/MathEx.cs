// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

internal static class MathEx
{
    public static int RoundUpToPowerOf2(int x)
    {
        // https://graphics.stanford.edu/~seander/bithacks.html#RoundUpPowerOf2
        x--;
        x |= x >> 1;
        x |= x >> 2;
        x |= x >> 4;
        x |= x >> 8;
        x |= x >> 16;
        x++;

        return x;
    }
}
