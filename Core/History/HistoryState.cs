using System.Collections.Generic;
namespace History
{
    [System.Serializable]
    public class HistoryState 
    {
        public List<CharacterData> characters;
        public List<GraphicData> graphics;
        public DialogueData dialogue;


        public static HistoryState Capture()
        {
            HistoryState state = new HistoryState();
            state.characters = CharacterData.Capture();
            state.graphics = GraphicData.Capture();
            state.dialogue = DialogueData.Capture();

            return state;
        }
        public void Load()
        {
            DialogueData.Apply(dialogue);
            CharacterData.Apply(characters);
            GraphicData.Apply(graphics);

        }
        

    }
}