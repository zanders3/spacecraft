using UnityEngine;
using System.Linq;

public class PlayerInventory
{
    public BlockType SelectedItem { get; private set; }

    public PlayerInventory()
    {
        SelectedItem = inventory[0];
    }

    BlockType[] inventory = new BlockType[]
    {
        BlockType.Steel,
        BlockType.PowerCore,
        BlockType.PilotSeat,
        BlockType.Thruster
    };

    public void DrawGUI()
    {
        float top = Screen.height - 80.0f, left = 10.0f;

        for (int i = 0; i<inventory.Length; i++)
        {
            GUIHelper.DrawQuad(new Rect(left + 100.0f*i, top, 80.0f, 30.0f), inventory[i] == SelectedItem ? Color.blue : Color.gray);
            GUI.Label(new Rect(left + 100.0f*i + 10.0f, top + 5.0f, 80.0f, 30.0f), inventory[i].ToString());
        }
    }

    public void Update()
    {
        for (int i = 0; i<inventory.Length; i++)
            if (Input.GetKeyDown((i+1).ToString()))
                SelectedItem = inventory[i];
    }
}
