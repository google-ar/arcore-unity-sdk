#import <ARKit/ARKit.h>

@interface ARSessionCast : NSObject <ARSessionDelegate>
{
@public
   ARSession* _session;
}
@end

@implementation ARSessionCast

extern "C" ARSession* ARCoreARKitIntegration_castUnitySessionToARKitSession(void* sessionToCast)
{
   ARSessionCast *nativeSession = (__bridge ARSessionCast*)(sessionToCast);
   ARSession* session = nativeSession->_session;

   return session;
}

extern "C" ARFrame* ARCoreARKitIntegration_getCurrentFrame(ARSession* arkitSession)
{
   return arkitSession.currentFrame;
}

@end
