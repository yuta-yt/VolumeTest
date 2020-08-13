#define INITIAL_DIST 1e10f

StructuredBuffer<uint> _SDFBuffer;

float3 _gridCenter;
float _cellSize;
float3 _cellCount;

// Get SDF cell index coordinates (x, y and z) from a point position in world space
int3 GetSdfCoordinates(float3 positionInWorld)
{
    float3 sdfPosition = (positionInWorld - _gridCenter) / _cellSize;
    
    int3 result;
    result.x = (int)sdfPosition.x;
    result.y = (int)sdfPosition.y;
    result.z = (int)sdfPosition.z;
    
    return result;
}

float3 GetSdfCellPosition(int3 gridPosition)
{
    float3 cellCenter = float3(gridPosition.x, gridPosition.y, gridPosition.z) * _cellSize;
    cellCenter += _gridCenter.xyz;
    
    return cellCenter;
}

int GetSdfCellIndex(int3 gridPosition)
{
    int cellsPerLine = _cellCount.x;
    int cellsPerPlane = _cellCount.x * _cellCount.y;

    return cellsPerPlane*gridPosition.z + cellsPerLine*gridPosition.y + gridPosition.x;
}

float LinearInterpolate(float a, float b, float t)
{
    return a * (1.0f - t) + b * t;
}

//    bilinear interpolation
//
//         p    :  1-p
//     c------------+----d
//     |            |    |
//     |            |    |
//     |       1-q  |    |
//     |            |    |
//     |            x    |
//     |            |    |
//     |         q  |    |
//     a------------+----b
//         p    :  1-p
//
//    x = BilinearInterpolate(a, b, c, d, p, q)
//      = LinearInterpolate(LinearInterpolate(a, b, p), LinearInterpolate(c, d, p), q)
float BilinearInterpolate(float a, float b, float c, float d, float p, float q)
{
    return LinearInterpolate( LinearInterpolate(a, b, p), LinearInterpolate(c, d, p), q );
}

//    trilinear interpolation
//
//                      c        p            1-p    d
//                       ------------------+----------
//                      /|                 |        /|
//                     /                   |       / |
//                    /  |                 |1-q   /  |
//                   /                     |     /   |
//                  /    |                 |    /    |
//               g ------------------+---------- h   |
//                 |     |           |     |   |     |
//                 |                 |     +   |     |
//                 |     |           |   r/|   |     |
//                 |                 |   / |q  |     |
//                 |     |           |  x  |   |     |
//                 |   a - - - - - - | / - + - |- - -| b
//                 |    /            |/1-r     |     /
//                 |                 +         |    /
//                 |  /              |         |   /
//                 |                 |         |  /
//                 |/                |         | /
//                 ------------------+----------
//              e                            f
//
//		x = TrilinearInterpolate(a, b, c, d, e, f, g, h, p, q, r)
//		  = LinearInterpolate(BilinearInterpolate(a, b, c, d, p, q), BilinearInterpolate(e, f, g, h, p, q), r)
float TrilinearInterpolate(float a, float b, float c, float d, float e, float f, float g, float h, float p, float q, float r)
{
    return LinearInterpolate(BilinearInterpolate(a, b, c, d, p, q), BilinearInterpolate(e, f, g, h, p, q), r);
}

// Get signed distance at the position in world space
float GetSignedDistance(float3 positionInWorld)
{
    int3 gridCoords = GetSdfCoordinates(positionInWorld);
    
    if( !(0 <= gridCoords.x && gridCoords.x < _cellCount.x - 2)
     || !(0 <= gridCoords.y && gridCoords.y < _cellCount.y - 2)
     || !(0 <= gridCoords.z && gridCoords.z < _cellCount.z - 2) ) 
        return INITIAL_DIST;
    
    int sdfIndices[8];
    {
        int index = GetSdfCellIndex(gridCoords);
        for(int i = 0; i < 8; ++i) sdfIndices[i] = index;
        
        int x = 1;
        int y = _cellCount.x;
        int z = _cellCount.y * _cellCount.x;
        
        sdfIndices[1] += x;
        sdfIndices[2] += y;
        sdfIndices[3] += y + x;
        
        sdfIndices[4] += z;
        sdfIndices[5] += z + x;
        sdfIndices[6] += z + y;
        sdfIndices[7] += z + y + x;
    }
    
    float distances[8];

    for(int j = 0; j < 8; ++j)
    {
        int sdfCellIndex = sdfIndices[j];
        float dist = asfloat(_SDFBuffer[sdfCellIndex]);

        if(dist == INITIAL_DIST) 
            return INITIAL_DIST;
        
        distances[j] = dist;
    }
    
    float distance_000 = distances[0];	// X,  Y,  Z
    float distance_100 = distances[1];	//+X,  Y,  Z
    float distance_010 = distances[2];	// X, +Y,  Z
    float distance_110 = distances[3];	//+X, +Y,  Z
    float distance_001 = distances[4];	// X,  Y, +Z
    float distance_101 = distances[5];	//+X,  Y, +Z
    float distance_011 = distances[6];	// X, +Y, +Z
    float distance_111 = distances[7];	//+X, +Y, +Z
    
    float3 cellPosition = GetSdfCellPosition(gridCoords);
    float3 interp = (positionInWorld - cellPosition) / _cellSize;
    return TrilinearInterpolate(distance_000, distance_100, distance_010, distance_110,
                                distance_001, distance_101, distance_011, distance_111,
                                interp.x, interp.y, interp.z);
}