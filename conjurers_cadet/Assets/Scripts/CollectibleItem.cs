public abstract class CollectibleItem : InteractiveItem {

	public override void interacted_with ( MainPlayerController player ) { self_collect_by( player ); }

	public virtual void self_collect_by ( MainPlayerController player ) { Destroy( gameObject ); }

}