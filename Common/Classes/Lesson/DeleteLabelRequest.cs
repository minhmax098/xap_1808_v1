using System;

[Serializable]
public class DeleteLabelRequest
{
    public int labelId;
    public void Init(int _labelId)
    {
        labelId = _labelId;
    }
}
