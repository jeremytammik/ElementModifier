# ElementModifier

Revit C# .NET add-in that applies a specified modification to an element, e.g., move, delete, change type.

ElementModifier implements an external command that receives a set of commands and applies them.

The commands are read from a JSON input file `C:\tmp\element_modifier_tasks.txt` specifying the actions to apply.

The add-in attempts to apply them to the BIM and reports both successful processing and any problems encountered back to the log file.

Here is a subset of defined commands:

- move_element_within_host(unique_id, new_position) &ndash; do not change the host element
- move_hostless_element(unique_id, new_position)
- delete_element(unique_id)
- change_element_type(unique_id, old_type_id, new_type_id)

The operations are reversible, i.e., can be undone, e.g., using Ctrl + Z.

If any problem crops up executing the operation in the BIM (and it will!) the operation is simply cancelled and the failure reported back to `C:\tmp\element_modifier_log.txt`.


## Author

Jeremy Tammik, [The Building Coder](http://thebuildingcoder.typepad.com), [ADN](http://www.autodesk.com/adn) [Open](http://www.autodesk.com/adnopen), [Autodesk Inc.](http://www.autodesk.com)


## License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT).
Please see the [LICENSE](LICENSE) file for full details.

