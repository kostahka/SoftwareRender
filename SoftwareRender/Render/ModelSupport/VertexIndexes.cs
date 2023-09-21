namespace SoftwareRender.Render.ModelSupport
{
    internal struct VertexIndexes
    {
        public VertexIndexes(int v_i, int t_i = 0, int n_i = 0)
        {
            this.v_i = v_i;
            this.t_i = t_i;
            this.n_i = n_i;
        }
        public int v_i;
        public int t_i;
        public int n_i;
    }
}
