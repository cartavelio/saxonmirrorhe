using Saxon.Hej.tree.linked;
using SysText = System.Text;
namespace Saxon.Ejava.text {
    public class Normalizer {
        public enum Form {
            NFC,
            NFD,
            NFKC,
            NFKD
        };

        public static string normalize(string str, Form form) {
            return str.Normalize(translate(form));
        }

        private static SysText.NormalizationForm translate(Form form) {
            switch (form) {
                case Form.NFC:
                    return SysText.NormalizationForm.FormC;
                case Form.NFD:
                    return SysText.NormalizationForm.FormD;
                case Form.NFKC:
                    return SysText.NormalizationForm.FormKC;
                case Form.NFKD:
                    return SysText.NormalizationForm.FormKD;
                default:
                    return SysText.NormalizationForm.FormC;
            }
        }

    }
}