#import "run-with-mono.h"

NSString * const ASSEMBLY = @"Arak.Data.Protection.exe";
NSString * const APP_NAME = @"Arak";
int const MONO_VERSION_MAJOR = 5;
int const MONO_VERSION_MINOR = 0;

int main() {
	@autoreleasepool {
		return [RunWithMono runAssemblyWithMono:APP_NAME assembly:ASSEMBLY major:MONO_VERSION_MAJOR minor:MONO_VERSION_MINOR];
	}
}

