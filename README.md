# DominoPlanner
Domino Event Design made easy

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
 
![calculation](https://user-images.githubusercontent.com/12698538/110957181-aa6d2e80-834b-11eb-8868-769e27859ad3.png)

### Editing
Any structure can be edited using the user-friendly integrated editor. It features
 * color replacement (press q for quick access)
 * different selection modes (rectangle, circle, polygon, fill bucket)
 * insert rows/columns into fields and regular structures
 * full undo/redo
 * a list of currently used colors
 * a measurement tool 

![editing](https://user-images.githubusercontent.com/12698538/110957422-e607f880-834b-11eb-974a-92d6739854b6.png)

### Project management
Domino structures are organized in Projects, and each project contains a color list. The color list - together with the amount of used dominoes - can be exported as .xlsx file.

![color list](https://user-images.githubusercontent.com/12698538/110957698-28c9d080-834c-11eb-86b1-c77fc0f77cfa.png

### Field Protocol & Field Viewer
For Fields and Regular structures, DominoPlanner can compute a field protocol. This field plan can be styled in many different ways and can be exported to Excel and as html file. It also includes separators for templates.

![field protocol](https://user-images.githubusercontent.com/12698538/110958059-8fe78500-834c-11eb-9b4a-0df40312b3c5.png)

To assist building, DominoPlanner can directly display the dominoes of the field, separated for each template. This is - in our experience - by far the most efficient way to build fields. 

![block viewer](https://user-images.githubusercontent.com/12698538/110959770-53b52400-834e-11eb-88ac-2d299297ada0.png)

## Download and Install
DominoPlanner is currently in public beta, so bugs and crashes may occur more often than in other programs. We are continuously working towards a stable version.
Knowing this, you can check out the [releases](https://github.com/jhofinger/DominoPlanner/releases) here. 
 * An installer for Windows and a MacOS image is bundled with each release.
 * You can also clone the git repository and compile it for yourself. DominoPlanner has been tested to work on Ubuntu 20.04.

## Feature suggestions and bug reports
We are always delighted to hear your feedback, so we can continously work towards one DominoPlanner for the entire community. Please use [GitHub issues](https://github.com/jhofinger/DominoPlanner/issues) for this.

You can also contribute by adding translations. Use the script https://github.com/jhofinger/DominoPlanner/blob/master/DominoPlanner.Usage/UpdateTranslations.ps1 for this to generate the po file for your language, translate it using PoEdit, Lokalize or GTranslator and consider to submit a merge request afterwards!

