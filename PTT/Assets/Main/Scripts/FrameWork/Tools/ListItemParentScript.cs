using UnityEngine;
using System.Collections;

public class ListItemParentScript : MonoBehaviour
{
    protected int myId;

    public virtual void SetId(int _myId)
    {
        myId = _myId;
    }
}
