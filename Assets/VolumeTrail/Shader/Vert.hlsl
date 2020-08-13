// Vertex input attributes
struct Attributes
{
    uint vertexID : SV_VertexID;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

// Vertex input attributes
struct AttributesToGS
{
    uint vertexID : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

// Custom vertex shader
AttributesToGS CustomVert(Attributes input)
{
    AttributesToGS attr;

    attr.vertexID = input.vertexID;
    UNITY_TRANSFER_INSTANCE_ID(input, attr);

    return attr;
}