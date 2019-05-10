# DominoPlanner
Domino Event Designing made easy

## Features
### Field calculation
DominoPlanner can calculate fields, structures, spirals and circles by analyzing images. Of course, different color repositories (Lamping, Bulk etc) can be used, but you can also create an own color list and adapt it to your own color names, for example.

The possible structures include
 * Fields (horizontal/vertical) with different domino dimensions (eg. turtle field, shingle field, fallwall...)
 * Regular structures (Walls, T-Wall, Edge wall, diagonal field, cube side, ...). Additional structure types can be added using an XML interface 
 * Circle bombs
 * Spirals

The color recognition algorithm can be finetuned to include
 * different color regression algorithms
 * Error correction (dithering)
 * Iterative color matching (respects maximum amount per color)
 * Transparent (empty) dominoes
 
 ![calculation](https://i.imgur.com/J1lwVwd.png)

### Field editing
Any structure can be edited using the user-friendly integrated editor. It features
 * color replacement
 * boolean selection
 * insert rows/columns into fields and regular structures
 * full undo/redo
 * a list of currently used colors

![editing](https://i.imgur.com/NawlRLN.png)

### Project management
Domino structures are organized in Projects, and each project contains a color list. The color list - together with the amount of used dominoes - can be exported as .xlsx file.

![color list](https://i.imgur.com/2aA6W6s.png)

### Field Protocol & Field Viewer
For Fields and Regular structures, DominoPlanner can compute a field protocol. This field plan can be styled in many different ways and can be exported to Excel and as html file. It also includes separators for templates.

![field protocol](https://i.imgur.com/POW8kiS.png)

For events, DominoPlanner can directly display the dominoes of the field, separated for each template. This is - in our experience - by far the most efficient way to build fields. 

![field viewer](https://i.imgur.com/e4XpX9z.png)

## Download and Install
DominoPlanner is currently in Alpha, so bugs and crashes may occur more often than in other programs. We are continuously working towards a stable version.
Knowing this, you can check out the [releases](https://github.com/jhofinger/DominoPlanner/releases) here. An installer for Windows is bundled with each release. You can also clone the git repository and compile it for yourself.

## Feature suggestions and bug reports
We are always delighted to hear your feedback, so we can continously work towards one DominoPlanner for the entire community. Please use [GitHub issues](https://github.com/jhofinger/DominoPlanner/issues) for this.

