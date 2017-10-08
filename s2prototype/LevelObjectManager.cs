using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace IntelOrca.Sonic
{
	class LevelObjectManager : IEnumerable<LevelObject>
	{
		private bool mLockCollection;
		private List<LevelObject> mObjects = new List<LevelObject>();
		private List<LevelObject> mNewObjects = new List<LevelObject>();

		public void Add(LevelObject obj)
		{
			if (mLockCollection)
				mNewObjects.Add(obj);
			else
				mObjects.Add(obj);
		}

		public void Update()
		{
			mLockCollection = true;
			foreach (LevelObject obj in mObjects)
				if (!obj.Finished && (obj is Character))
					obj.Update();

			foreach (LevelObject obj in mObjects)
				if (!obj.Finished && !(obj is Character))
					obj.Update();
			mLockCollection = false;

			// Add new objects
			AddNewObjects();

			// Remove finished objects
			RemoveFinishedObjects();
		}

		public void Draw(Graphics g, Rectangle view, int minPriority, int maxPriority)
		{
			mLockCollection = true;
			foreach (LevelObject obj in mObjects.OrderBy(o => o.DrawPriority)) {
				if (obj.DrawPriority < minPriority || obj.DrawPriority > maxPriority)
					continue;

				if (obj.DisplacementX < view.X - 512 || obj.DisplacementX > view.X + view.Width + 512)
					continue;
				if (obj.DisplacementY < view.Y - 512 || obj.DisplacementY > view.Y + view.Width + 512)
					continue;

				int tx = (obj.DisplacementX - view.X) * 4;
				int ty = (obj.DisplacementY - view.Y) * 4;

				float pX = (float)BitConverter.ToUInt32(BitConverter.GetBytes(obj.PartialDisplacementX), 0) / (float)UInt32.MaxValue;
				float pY = (float)BitConverter.ToUInt32(BitConverter.GetBytes(obj.PartialDisplacementY), 0) / (float)UInt32.MaxValue;
				tx += (int)(pX * 4.0f);
				ty += (int)(pY * 4.0f);

				g.Translate(tx, ty);
				obj.Draw(g);
				g.Translate(-tx, -ty);
			}
			mLockCollection = false;
		}

		public void Clear()
		{
			mObjects.Clear();
			mNewObjects.Clear();
		}

		public IEnumerable<LevelObject> GetObjectsInArea(Rectangle area)
		{
			return mObjects.Where(o => o.Bounds.Intersects(area) && !o.Finished);
		}

		private void AddNewObjects()
		{
			mObjects.AddRange(mNewObjects);
			mNewObjects.Clear();
		}

		private void RemoveFinishedObjects()
		{
			mObjects.RemoveAll(o => o.Finished);
		}

		public IEnumerator<LevelObject> GetEnumerator()
		{
			return mObjects.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return mObjects.GetEnumerator();
		}
	}
}
