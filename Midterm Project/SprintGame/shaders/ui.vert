#version 330 core
    layout(location = 0) in vec2 aPos;
    uniform vec2 offset;
    uniform vec2 scale;
    void main()
    {
        vec2 pos = aPos * scale + offset;
        gl_Position = vec4(pos, 0.0, 1.0);
    }