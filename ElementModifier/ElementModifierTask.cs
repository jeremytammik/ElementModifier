using Autodesk.Revit.DB;

namespace ElementModifier
{
  class ElementModifierTask
  {
    public ElementId Id { get; set; }
  }

  class ElementModifierTaskDelete : ElementModifierTask
  {
  }

  class ElementModifierTaskMove : ElementModifierTask
  {
    public XYZ Translation { get; set; }
  }

  class ElementModifierTaskSetType : ElementModifierTask
  {
    public ElementId TypeId { get; set; }
  }
}
