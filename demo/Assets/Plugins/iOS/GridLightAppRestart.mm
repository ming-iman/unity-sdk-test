#import <Foundation/Foundation.h>
#import <stdlib.h>

extern "C" void gridlight_exit_application(void)
{
    dispatch_async(dispatch_get_main_queue(), ^{
        exit(0);
    });
}
