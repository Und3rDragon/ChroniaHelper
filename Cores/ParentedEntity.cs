using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChroniaHelper.Cores;

public class ParentedEntity : BaseEntity
{
    public ParentedEntity(EntityData d, Vc2 o) : base(d, o)
    {
        
    }
    public ParentedEntity parent = null;
    public List<ParentedEntity> members = new();
    public Dictionary<string, List<ParentedEntity>> subMembers = new();

    protected override void AwakeExecute(Scene scene)
    {
        // Usually after all entities are added, we can start finding entity parent
        FindParent(scene);
    }
    /// <summary>
    /// The usual function for finding parent entity and filling members or sub-members
    /// </summary>
    protected virtual void FindParent(Scene scene) { }

    protected override void AddedExecute(Scene scene)
    {
        FindParentWhenAdded(scene);
    }
    /// <summary>
    /// Usually this is not suggested, you should use FindParent()
    /// </summary>
    protected virtual void FindParentWhenAdded(Scene scene) { }
}
