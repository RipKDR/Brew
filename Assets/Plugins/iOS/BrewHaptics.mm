#import <UIKit/UIKit.h>

extern "C" {

void Brew_IOS_Impact(int style) {
  if (@available(iOS 10.0, *)) {
    UIImpactFeedbackStyle feedbackStyle = UIImpactFeedbackStyleLight;
    if (style == 1)
      feedbackStyle = UIImpactFeedbackStyleMedium;
    else if (style >= 2)
      feedbackStyle = UIImpactFeedbackStyleHeavy;

    UIImpactFeedbackGenerator *generator =
        [[UIImpactFeedbackGenerator alloc] initWithStyle:feedbackStyle];
    [generator prepare];
    [generator impactOccurred];
  }
}

void Brew_IOS_Notification(int type) {
  if (@available(iOS 10.0, *)) {
    UINotificationFeedbackType feedbackType = UINotificationFeedbackTypeSuccess;
    if (type == 1)
      feedbackType = UINotificationFeedbackTypeWarning;
    else if (type == 2)
      feedbackType = UINotificationFeedbackTypeError;

    UINotificationFeedbackGenerator *generator =
        [[UINotificationFeedbackGenerator alloc] init];
    [generator prepare];
    [generator notificationOccurred:feedbackType];
  }
}

void Brew_IOS_Selection(void) {
  if (@available(iOS 10.0, *)) {
    UISelectionFeedbackGenerator *generator =
        [[UISelectionFeedbackGenerator alloc] init];
    [generator prepare];
    [generator selectionChanged];
  }
}
}
