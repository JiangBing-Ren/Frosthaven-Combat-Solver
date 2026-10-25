public enum ConnectionErrorCode
{
	[Name("None", "Unknown Error Occured.")]
	None,
	[Name("Different Build Type", "Game build types do not match.")]
	DifferentBuildType,
	[Name("Different Version", "Game versions do not match.")]
	DifferentVersion,
	[Name("No Session Found", "Timed out searching for the session.")]
	SessionNotFound,
	[Name("Session Full", "The session is full.")]
	SessionFull,
	[Name("User Blocked", "The host has blocked you from the session.")]
	UserBlocked,
	[Name("Server Refused", "The host has refused connectiong you to session.")]
	ServerRefused,
	[Name("Incorrect Password", "The password is incorrect.")]
	IncorrectPassword,
	[Name("Invalid Code", "The code used is invalid.")]
	InvalidCode,
	[Name("Invalid User Data", "The user data provided is invalid.")]
	InvalidUserData,
	[Name("Invalid File Path", "Could not process the target save file path. Disconnecting.")]
	InvalidFilePath,
	[Name("Invalid Session Data", "Invalid session data provided. Disconnecting.")]
	InvalidSessionData,
	[Name("Invalid Game Mode", "Unsuitable or unfinished game mode found.")]
	InvalidGameMode,
	[Name("Connection To Backend Failed", "Could connect to the multiplayer backend.")]
	ConnectionToBackendFailed,
	[Name("Connection To Session Failed", "Could not connect to the session.")]
	ConnectionToSessionFailed,
	[Name("Mods do not match", "The host has mods enabled.  All clients must have the same mods.")]
	ModsDoNotMatch,
	[Name("Session Shutting Down", "The session is shutting down")]
	SessionShuttingDown,
	[Name("DLC do not match", "The host has DLC enabled. All clients must have the same DLC.")]
	DLCDoNotMatch
}
