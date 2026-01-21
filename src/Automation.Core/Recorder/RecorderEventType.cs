using System.Runtime.Serialization;

namespace Automation.Core.Recorder;

public enum RecorderEventType
{
    [EnumMember(Value = "navigate")]
    Navigate,
    [EnumMember(Value = "click")]
    Click,
    [EnumMember(Value = "fill")]
    Fill,
    [EnumMember(Value = "select")]
    Select,
    [EnumMember(Value = "toggle")]
    Toggle,
    [EnumMember(Value = "submit")]
    Submit,
    [EnumMember(Value = "modal_open")]
    ModalOpen,
    [EnumMember(Value = "modal_close")]
    ModalClose
}
