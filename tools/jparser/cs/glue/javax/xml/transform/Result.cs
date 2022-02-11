namespace javax.xml.transform {
    public interface Result {
        private string sysId;
        public string getSystemId() {
            return sysId;
        }

        public void setSystemId(string systemId) {
            this.sysId = systemId;
        }
    }
}