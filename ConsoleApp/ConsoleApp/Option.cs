public class Option
{
    public bool IsPlayBGM { get; private set; } = false;
    public bool IsEffectBGM { get; private set; } = false;
    public bool IsPush { get; private set; } = false;

    public class Builder
    {
        private Option option = new Option();

        public static Builder Create()
        {
            return new Builder();
        }

        public Builder SetPlayBGM(bool isOn)
        {
            option.IsPlayBGM = isOn;
            return this;
        }

        public Builder SetEffectBGM(bool isOn)
        {
            option.IsEffectBGM = isOn;
            return this;
        }

        public Builder SetPush(bool isOn)
        {
            option.IsPush = isOn;
            return this;
        }

        public Option Build()
        {
            return this.option;
        }
    }
}