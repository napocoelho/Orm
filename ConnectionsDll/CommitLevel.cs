namespace Connections
{
    public class CommitLevel
    {
        private int _value_;

        public int Level
        {
            get
            {
                return _value_;
            }
        }

        public CommitLevel()
        {
            _value_ = 0;
        }

        public void Up()
        {
            _value_ = _value_ + 1;
        }

        public void Down()
        {

            // não deixa ficar negativo:
            if (_value_ < 1)
            {
                _value_ = 0;
            }
            else
            {
                _value_ -= 1;
            }
        }

        public void Reset()
        {
            _value_ = 0;
        }
    }
}