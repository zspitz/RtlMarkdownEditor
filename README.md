# RTL Markdown Editor

This is a Markdown editor for writing RTL prose.

* VS Code respects the bidi algorithm and Unicode bidi control characters, but doesn't allow switching the entire editor to RTL
* Notepad++ allows switching, but doesn't respect the Unicode bidi algorithm.

Specific goals:

* Enable switching the entire editor from LRT to RTL and back. This should not change the Markdown. But preview should follow the editor direction.
* Enable the user to apply direction reversal on a specific selection.
  * Inserts a starting control character before the selection (depending on current context), and a PDI after the selection
    * If selection start is LTR, insert RLI before selection
    * If RTL, insert LRI before selecttion
    * If neutral, insert FSI
  * Command can't be used on a selection that spans multiple block elements, e.g. across multiple paragraphs
* Display the directional character codes as special characters
* Show direction of character before and after current selection in status bar: →→, →←, ←←, ←→

Currently:

* RTL editor and preview (no toggle)
* No dislay of special Unicode characters (https://github.com/Ionaru/easy-markdown-editor/issues/427)
