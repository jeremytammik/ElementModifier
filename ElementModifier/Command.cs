#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace ElementModifier
{
  [Transaction( TransactionMode.Manual )]
  public class Command : IExternalCommand
  {
    /// <summary>
    /// Input file specifying tasks to execute
    /// </summary>
    const string _input_file_path
      = "C:/tmp/element_modifier_tasks.txt";

    /// <summary>
    /// Log messages file path
    /// </summary>
    const string _log_file_path
      = "C:/tmp/element_modifier_log.txt";

    /// <summary>
    /// Format task arguments using parenthesis or commata
    /// </summary>
    static char[] separators = new char[] { '(', ')', ',', ' ' };

    /// <summary>
    /// Parse an element id from a string
    /// </summary>
    static bool ParseId( string s, out ElementId id )
    {
      int i = int.Parse( s );
      id = new ElementId( i );
      return true;
    }

    /// <summary>
    /// Parse an XYZ point from three strings
    /// </summary>
    static bool ParseXyz( string sx, string sy, string sz, out XYZ p )
    {
      double x = double.Parse( sx );
      double y = double.Parse( sy );
      double z = double.Parse( sz );
      p = new XYZ( x, y, z );
      return true;
    }

    /// <summary>
    /// Read input file and return list of tasks
    /// </summary>
    static List<ElementModifierTask> ParseTasks(
      Document doc,
      string filepath )
    {
      if( !File.Exists( filepath ) )
      {
        throw new Exception( string.Format( 
          "Input file '{0}' not found.", 
          filepath ) );
      }

      string[] lines = File.ReadAllLines( filepath );

      int lineno = 0;

      List<ElementModifierTask> tasks 
        = new List<ElementModifierTask>();

      foreach( string line in lines )
      {
        ++lineno;

        string s = line.Trim();

        if( 0 == s.Length // skip empty line
          || s[ 0 ].Equals( '#' ) ) // skip comment
        {
          continue;
        }

        string[] tokens = s.ToLower().Split( separators );

        if( 2 > tokens.Length )
        {
          throw new Exception( string.Format(
            "Input file '{0}' line {1} is incomplete: {2}.",
            filepath, lineno, s ) );
        }

        Element e = doc.GetElement( tokens[ 1 ] );

        if( null == e )
        {
          throw new Exception( string.Format(
            "Input file '{0}' line {1} invalid unique id: {2}.",
            filepath, lineno, tokens[ 1 ] ) );
        }

        if( tokens[ 0 ].Equals( "delete" ) )
        {
          ElementModifierTaskDelete task = new ElementModifierTaskDelete();
          task.Id = e.Id;
          tasks.Add( task );
        }
        else if( tokens[ 0 ].Equals( "move" ) )
        {
          if( null == e.Location
            || !(e.Location is LocationPoint))
          {
            throw new Exception( string.Format(
              "Input file '{0}' line {1} invalid move target element: {2}.",
              filepath, lineno, tokens[ 1 ] ) );
          }

          XYZ p = ((LocationPoint) e.Location).Point;

          XYZ q;
          if( 5 != tokens.Length
            || !ParseXyz( tokens[ 2 ], tokens[ 3 ], tokens[ 4 ], out q ) )
          {
            throw new Exception( string.Format(
              "Input file '{0}' line {1} invalid move command arguments: {2}.",
              filepath, lineno, s ) );
          }
          ElementModifierTaskMove task = new ElementModifierTaskMove();
          task.Id = e.Id;
          task.Translation = q - p;
          tasks.Add( task );
        }
        else if( tokens[0].Equals("settype") )
        {
          ElementId typeId;
          if( 3 != tokens.Length 
            || !ParseId( tokens[ 2 ], out typeId ) )
          {
            throw new Exception( string.Format(
              "Input file '{0}' line {1} invalid set type command arguments: {2}.",
              filepath, lineno, s ) );
          }
          ElementModifierTaskSetType task = new ElementModifierTaskSetType();
          task.Id = e.Id;
          task.TypeId = typeId;
          tasks.Add( task );
        }
        else
        {
          throw new Exception( string.Format(
            "Input file '{0}' line {1} invalid command: {2}.",
            filepath, lineno, s ) );

        }
      }
      return tasks;
    }


    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements )
    {
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Application app = uiapp.Application;
      Document doc = uidoc.Document;

      List<ElementModifierTask> tasks;

      try
      {
        tasks = ParseTasks( doc, _input_file_path );
      }
      catch(Exception ex)
      {
        message = ex.Message;
        File.WriteAllText( _log_file_path, message );
        return Result.Failed;
      }

      List<string> log_messages = new List<string>( 
        tasks.Count );

      int i = 0;
      foreach( ElementModifierTask task in tasks )
      {
        ++i;

        using( Transaction tx = new Transaction( doc ) )
        {
          tx.Start( string.Format( 
            "Task {0}: {1}", i, task ) );

          if( task is ElementModifierTaskDelete )
          {
            doc.Delete( task.Id );

            log_messages.Add( string.Format( 
              "Deleted {0}", task.Id ) );
          }
          else if( task is ElementModifierTaskMove )
          {
            XYZ v = ((ElementModifierTaskMove) task)
              .Translation;

            ElementTransformUtils.MoveElement(
              doc, task.Id, v );

            log_messages.Add( string.Format(
              "Moved {0}", task.Id ) );
          }
          else if( task is ElementModifierTaskSetType )
          {
            ElementId typeId 
              = ((ElementModifierTaskSetType) task).TypeId;

            Element e = doc.GetElement( task.Id );
            e.ChangeTypeId( typeId );

            log_messages.Add( string.Format(
              "Set type for {0}", task.Id ) );
          }
          tx.Commit();
        }
      }

      File.WriteAllLines( _log_file_path, log_messages );

      return Result.Succeeded;
    }
  }
}
