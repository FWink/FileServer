# FileServer
Mimicking IIS directory browsing with added bulk download of multiple files in a zip archive

# Usage
Compile and publish to an IIS server. All files within the application's "wwwroot" directory may be accessed by clients (symlinks/junctions are being followed)

There is no kind of access control; any client that is able to access the application can access any file it hosts.
