using Autodesk.Revit.DB;

namespace ElementModifier
{
  class ElementModifierTask
  {
    public string UniqueId { get; set; }
  }

  class ElementModifierTaskMove : ElementModifierTask
  {
    public XYZ Translation { get; set; }
  }

  class ElementModifierTaskSetType : ElementModifierTask
  {
    public ElementId NewTypeId { get; set; }
  }
}
