# DominoPlanner Testing Script

## Install and First start
0. Clean Appdata manually.
1. [Windows and Mac OS]: Run installer. Make sure the app is registered correctly (Start menu, application menu)
1. Open App.
    1. App should be in German on a German system.
    1. Message box "Create default color table" should be displayed. Click Ok.
    1. Click "Open color list". The folder containing the default color lists should open.
    1. Perform some edits (change color name, RGB value, count).
    1. Create a new color and move it upwards. Color should move as expected.
    1. Save the color list. The save button should be fully visible.
1. Open about window. 
    1. Click the version button. Make sure the version is copied to the clipboard.
    1. Click the bugtracker button and the support button. The browser should open the two pages correctly.

## Create a project
1. Click "New -> New Project". Click Ok. A new project should be visible in the project panel.
1. Right click the new project.
    1. Click "Open colors". The color list should open. Do not close the color list yet.
    1. Click "Rename". Enter a new name for the project. The project panel should update with the new name.
    1. Click "Add new object". Create a field with default settings. The new field should be displayed in the project panel and opened in a new tab.
    1. Click "Rename" again. Enter a new name again. The project panel should update. If an object of the project was open, it may close automatically (after prompting to save)
    1. Click "Export all", confirm and choose the folder of the project.
    1. Click "Open folder". Make sure that all files from the previous step are there.
    1. (Windows only, inside Explorer) Open "Planner files" and click on the created field file. Open the preview panel. Make sure that
        1. the DominoPlanner icon is shown on the bottom right, 
        1. a preview of the object is displayed.
    1. (Windows only) zoom in to make sure that the thumbnail is correctly displayed. Close explorer.
    1. Click "Properties". The property windows should open. Open "Obj", "children", "[0]", "Obj", "Colors". A "Warning" sign should be displayed next to "Item".
    1. Open the field if it is not open.
    1. Click "Remove".
1. Re-add the project using "Add" -> "Add existing project".
1. Click "Workspace". Make sure the field is still listed.

## Fields

### Creation 
1. Right-click the project and add new object.
1. (Windows and MacOS) Drag a non-Image file to the image area. A message box rejecting the image should appear.
1. (Windows and MacOS) Drag an image file to the area. The image should show up, and the file should path update.
1. Click the image area and open a non-image file. The image should be rejected.
1. Click the image area and open an image file. The image should show up, and the file path should update.
1. Switch between the different field presets. The dimensions should update.
1. In "Custom dimensions", change a setting. Switch to another preset and back to custom. The changed value should still be there.
1. Enter 10000 into the Field Size box. The dimensions should update and the resulting field size should be reasonably close to 10000.
1. Enter a file name and click Create. The field should show.
1. Switch the direction to vertical. The dominoes should reorient.
1. Click the two buttons in the drawing settings. The field should be redrawn.
1. Switch between different scaling qualities, color readout types and dithering types. The preview should update everytime.
1. Click the ✕ / ✓ sign next to "Match available colors". The used colors should be displayed.
1. Deactivate dithering and enable "Match available colors". If the restriction is already fulfilled, increase the field size moderately until there is a red ✕ next to the available colors. Increase the number of attempts and the penalty until the ✕ turns to a ✓.
1. Undo until you reach the initial settings.
1. Redo until you reach the same settings as before. Make sure that the field size is correct and the ✓ is still there.
1. Click "Edit". Undo. The color list should not appear. Redo. Editing should be activated again. 
    * There should be no spaces between the dominoes.
    * Name, Rows, Columns, Amout and Dimensions should be correct.
1. Create a field with a logo-type image that contains different alpha levels and transparent background. The Inkscape Logo ist a good example. The background should be white initially.
1. Hide the domino spaces. Set the background to a non-transparent / white color. Nothing should change (except drawing artifacts)
1. Move the Quality slider to Pixel and play with the Transparency threshold. You should see more dominoes become transparent / the color of the background.

### Build tools
1. The field should contain some "empty" dominoes. Open the Field plan. Export the fieldplan to Excel. Make sure that the empty dominoes are displayed correctly (diagonal background pattern).
1. Click the red circle. The two arrows should swap. 
1. Click another circle. The arrows should be adjacent to that circle. 
1. Open the Field viewer and confirm that the fieldplan matches the selected directions, i.e.: dark line = within one row, light line: rows.
1. Close the field viewer and switch the field to Vertical. 
1. Open the protocol view. The center should be at the bottom left corner; the dark arrow is vertical. 
1. Open the field viewer. Confirm that the arrows match the field plan.
1. Export the field plan to Excel. Confirm that the arrows match the field plan.

## Editing - General
### Selection Tool
1. Scroll, zoom and pan using the mouse (+ shift, Ctrl). 
1. Select a region using the left mouse button, deselect using the right mouse button. Make sure that the panel on the right updates after each operation.
1. Repeat the operation with selection mode "+". The dominoes should be selected with both mouse buttons.
1. Repeat the operation with selection mode "-". The dominoes should be deselected with both mouse buttons.
1. Perform a selection using all five selection modes, including and excluding the boundary / diagonals.
1. Invert the selection.
1. Clear the selection with Esc. 
1. Undo the clearing with the button in the toolbar.
1. Redo the clearing.
1. Select a region, and change the color by double-clicking another color in the color list. The count of the color should update.
1. Select a region, and change the color by pressing "r". A small popup should open where you can click a color.
1. Undo the last changes and clear the selection.
1. Select a color on the right and click "Select dominoes in selected color". Click another color and repeat. The selection should clear.
### Ruler Tool
1. Click and drag a ruler. Observe the Length, it should change while dragging.
1. Click one of the end points of the ruler and move it. 
1. Press Ctrl. Move the ruler again; it should snap in 5° increments.
1. Zoom in and out. The ruler should stay at the same position.
1. Click the Row/Column tool. The ruler should disappear.

### Row/Column Tool
1. Switch on the background image in the display settings (increase opacity and set to "above dominoes").
1. Activate Row/Column Tool.
1. Delete and add some rows/columns. The image should still be in the correct position. Amount and Dimensions should update.
1. Undo/Redo some of the previous operation and insert/delete more rows/columns. Make sure insertion/deletion also works at the beginning / end.
1. Undo all of the previous operations. 
1. Select some dominoes in different regions of the image using the Select tool. Delete the rows using Edit -> Delete -> Delete rows. Make sure the image is still correct.

### Zoom Tool
1. Click "Fit". The entire object should be visible.
1. Click the - and + buttons. The field should zoom in and out.
1. Move the ruler. The field should zoom in and out.

## Structures
1. Click New -> New object. Open an image and switch to structure. 
1. Switch between the different structure types. The total amount of dominoes should remain approximately constant.
1. Set a name and click OK.
1. Switch between single pixel and average color. With the second option, the structure should look smoother.
1. Increase the size to 5000. The aspect ratio should still be correct.
1. Increase the number of columns. With "Match aspect ratio", the image should distort. Without, the image should have the correct aspect ratio, and cut off at the bottom.
1. Undo the last few operations. The size should be close to 5000 again. 
1. Enable dithering. The preview should update.

### Editing
1. Make sure selection, and especially bucket tool selection, still works. 
1. Select some dominoes and copy them. The paste positions should be highlighted and a preview of the clipboard will move with the mouse.
1. Click somewhere next to the highlighted paste positions. Nothing should happen.
1. Hold Ctrl and click one of the paste positions. The dominoes should be pasted, but pasting mode will still be active. 
1. Release Ctrl and click another paste positions. Pasting mode should be disabled.
1. Enable the background image and activate Row/Column tool. Delete some rows/columns. The image should move along.
1. Close the structure and reopen it. Reactive the background image. The image should still be in the correct position, next to the corresponding dominoes.

### Structure Protocols
1. Go back to basic settings. Change structure type to Wall. Click Build tools. The center should be at the bottom left, with the dark blue line (line precedence) horizontal and the light blue line (row precedence) vertical.
1. Activate blocks and change the number of blocks per line to 25.
1. Activate Block viewer, enable history. Go to the rightmost block and check that 
    * the two rows have different length
    * the text is still there.
1. Use the arrow down key and confirm that the two conditions above are still fulfilled.
1. Change structure type to T-Wall. The "Show build tools" Button should disappear.
1. Change structure type to diagonal field. The button should reappear. 
1. Click Build tools. The field should be rotated 45° clockwise, with the center at the top left, and a horizontal dark blue arrow.
1. Confirm that the number of blocks per line is still 25.
1. Activate history and use the Arrow Down-Key. Confirm that the bottom line is always four dominoes longer than the top one (in the upper half at least).
1. Click the "Show remaining dominoes" button. A popup should open.
1. Click p. The "pin" button should be pressed now.
1. Go to a different block. Click "Go to last pin". Confirm that the position is identical to the pinned position.
1. Close Block viewer. Export the fieldplan to Excel. Excel should open.
1. Confirm that the blocksize is correct, and that the total amount of dominoes per line increases by four each line in the upper half.
1. Export to HTML. Compare the two saved fieldplans. Confirm sure that they are identical. Confirm that the HTML fieldplan is complete, i.e. not cut off at the bottom.

## File operations
1. Save the structure. The star next to the filename should disappear.
1. Change something within the structure. The star should reappear.
1. Right click the structure and rename it. You will be asked whether to save unsaved changes. Confirm; the structure should close.
1. Enter a new name. The structure should reopen automatically. Confirm that the name has been updated in the tab header and the project panel. Open Build tools; the name should be updated here as well.
1. Export as image. Confirm that the export is correct.
1. Export as custom image, change some settings. Confirm that the export matches the settings.
1. Check that Context Menu -> Show Structure protocol shows the protocol settings.
1. Remove the file from the project.
1. Re-add the file to the project (Add -> Add existing object). The file should open automatically.

## Color operations
1. Select a color and move it. It should move as expected. 
1. Delete a color that is used by two or more objects. It should be displayed as gray. 
1. Open one of the objects that use the color. Go to basic settings; change a setting to trigger a recalculation. Switch back to the color tab and confirm that the cell in the row of the color and the column of the object is 0.
1. Restore the color by clicking the Restore button. The font should be black again.
1. Delete a color that is not used by any project. Restore the color using undo.
1. Add a color and assign it a count != 0. 
1. Go to a project and enable dithering. (This is likely the easiest way to test this).
1. Confirm that the new color is listed in the color list. 
1. Confirm that the new color is used by the project.
1. Export the color list to Excel. Make sure that the new color is correctly displayed.

## Subprojects
1. Create a subproject.
1. Add a new project.
1. Try to add the parent project. An error message should be displayed.
1. Add one of the previously created objects. 
1. Open the same object from the list of objects within the parent project. It should not be opened again.
1. Open the color list of the parent project. The subproject should have a separate row in the header.
1. Export it to Excel. Make sure the two color lists are identical.
1. Close DominoPlanner. For each open project with unsaved changes, you should be prompted whether to save the changes. Cancel one of the message boxes. DominoPlanner should stay open.
1. Close DominoPlanner. This time, confirm the message box.

## OS Integration (Windows)
1. Double click a file from Explorer. DominoPlanner should open the file.
1. Remove the project. Double click the project in Explorer, it should be re-added. A second DominoPlanner window is not supposed to open.
1. Open a color list associated with the current project from Explorer. It should open and display the counts of the project.
1. Open a color list not associated with an open project (such as the default color list). It should open without any counts.