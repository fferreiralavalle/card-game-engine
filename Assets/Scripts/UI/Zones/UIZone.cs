using DG.Tweening;
using RuntimeCardEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Unity.IO.Archive;
using Unity.VisualScripting;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class UIZone : MonoBehaviour
{
    public string zoneCategory = "";
    public string ownerId = "";
    public float entitySeparation = 1f;
    public int baseSortOrder = 0;
    public Transform originPoint;
    public Transform previewZone;
    public float previewTime = 1f;
    public UICardEntity zoneCardEntityPrefab;

    public List<Transform> cards = new List<Transform>();

    public bool allowPlayFromZone = false;

    public Zone Zone;


    public virtual UIZone Initialize(Zone zone, Game game)
    {
        Zone = zone;
        HandleZoneMoveTriggers(game);
        foreach (Entity entity in zone.GetEntities())
        {
            HandleAddCard(entity, false, false);
        }
        return this;
    }

    public virtual void HandleZoneMoveTriggers(Game game)
    {
        OnMoveToZoneTrigger moveToZoneTrigger = new OnMoveToZoneTrigger(Zone.zoneCategory, Zone.ownerId);

        moveToZoneTrigger.onTrigger += (ev, trigger) =>
        {
            Func<Task> animationTask = async () =>
            {
                if (ev is MoveToZoneEvent moveEvent)
                {
                    await HandleCardsAdd(moveEvent);
                }
            };
            VisualSequencer.Instance.EnqueueAnimation(animationTask);
        };
        OnMoveFromZoneTrigger moveFromZoneTrigger = new OnMoveFromZoneTrigger(Zone.zoneCategory, Zone.ownerId);

        moveFromZoneTrigger.onTrigger += (ev, trigger) =>
        {
            Func<Task> animationTask = async () =>
            {
                if (ev is MoveToZoneEvent moveEvent)
                {
                    await HandleCardsRemove(moveEvent);
                }
            };
            VisualSequencer.Instance.EnqueueAnimation(animationTask);
        };
        game.AddTrigger(moveFromZoneTrigger);
        game.AddTrigger(moveToZoneTrigger);
    }

    public virtual async Task HandleCardsAdd(MoveToZoneEvent moveToZoneEvent)
    {
        foreach (Entity entity in moveToZoneEvent.movedEntities)
        {
            Func<Task> animationTask = async () =>
            {
                SetCardEntityOriginalPos(entity, moveToZoneEvent);
                await HandleAddCard(entity, false, false);
            };
            VisualSequencer.Instance.EnqueueAnimation(animationTask, -5);
        }
        foreach (Entity entity in moveToZoneEvent.overflownEntities)
        {
            SetCardEntityOriginalPos(entity, moveToZoneEvent);
            await HandleAddCard(entity, false, true);
        }
    }

    public virtual async Task HandleCardsRemove(MoveToZoneEvent moveToZoneEvent)
    {
        foreach (MoveZoneInfo moveInfo in moveToZoneEvent.moveZoneInfos.Values.ToList())
        {
            Entity e = moveInfo.entity;
            UICardEntity entityInHand = GetView(e);
            if (entityInHand != null)
            {
                cards.Remove(entityInHand.gameObject.transform);
                entityInHand.onDrag -= HandleDrag;
                entityInHand.onDrop -= HandleDrop;
            }
        }
        await LayoutCardsTask();
    }


    protected UICardEntity SetCardEntityOriginalPos(Entity entity, MoveToZoneEvent moveToZoneEvent)
    {
        MoveZoneInfo moveZoneInfo = moveToZoneEvent.GetEntityZoneInfo(entity.runtimeId);
        Zone origin = moveZoneInfo.originalZone;
        Transform originPosition = null;
        bool entityExisted = UIVisualManager.Instance.CardEntityExists(entity);
        UICardEntity uICardEntity = UIVisualManager.Instance.GetCardEntity(entity, zoneCardEntityPrefab);
        if (entityExisted)
        {
            originPosition = uICardEntity.transform;
        }
        else if (origin != null)
        {
            UIZone uiOrigin = UIZoneManager.Instance.GetZone(origin.zoneCategory, origin.ownerId);
            originPosition = uiOrigin.originPoint;
        }
        else
        {
            originPosition = previewZone?.transform ?? originPoint;
        }
        uICardEntity.transform.SetPositionAndRotation(originPosition.position, originPosition.rotation);

        return uICardEntity;
    }

    public virtual async Task HandleAddCard(Entity entity, bool showPreview, bool overDraw)
    {
        UICardEntity entityUI = UIVisualManager.Instance.TransformCardEntityPreset(entity, zoneCardEntityPrefab);
        if (allowPlayFromZone && !overDraw)
        {
            entityUI.onDrag += HandleDrag;
            entityUI.onDrop += HandleDrop;
        }
        await AddCardTask(entityUI.transform, showPreview, overDraw);
    }

    public virtual void HandleDrag(UICardEntity uiEntity)
    {
        uiEntity.AddComponent<FollowMouse>();
    }

    public virtual void HandleDrop(UICardEntity uiEntity)
    {
        Destroy(uiEntity.GetComponent<FollowMouse>());

        // TODO: Change Later
        bool isInPlayZone = UIZoneManager.Instance.IsInPlayZone(uiEntity.transform.position);
        if (isInPlayZone)
        {
            if (UIZoneManager.Instance.TryToPlay(uiEntity))
            {
                uiEntity.onDrag -= HandleDrag;
                uiEntity.onDrop -= HandleDrop;
            }
            else
            {
                LayoutCardsTask(uiEntity);
            }
        }
        else
        {
            LayoutCardsTask(uiEntity);
        }
    }

    public virtual async Task AddCardTask(Transform card, bool showPreview, bool overDraw)
    {
        if (showPreview)
        {
            await ShowPreviewTask(card);
        }
        if (overDraw)
        {
            await OverdrawCardTask(card);
        }
        else
        {
            cards.Add(card);
            LayoutCardsTask();
        }

    }

    public virtual async Task ShowPreviewTask(Transform card)
    {
        if (!previewZone) return;
        // Make sure preview is shown properly
        var canvas = card.GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = baseSortOrder;
        }

        var cardView = card.GetComponent<UICardEntity>();
        var mainCam = Camera.main;

        // 1. Create a Sequence
        Sequence seq = DOTween.Sequence();

        // 2. Join both tweens so they play at the SAME time
        seq.Join(card.DORotateQuaternion(previewZone.rotation, 0.5f));
        seq.Join(card.DOMove(previewZone.position, 0.5f));
        seq.AppendInterval(previewTime * 0.3f);

        seq.AppendCallback(async () =>
        {
            if (cardView != null)
            {
                cardView.UpdateCosts();
                await cardView.UpdateAttributes();
            }
        });

        // 3. Pause after the callback executes
        seq.AppendInterval(previewTime * 0.7f);

        // 4. OnUpdate runs on every frame while the sequence is active
        seq.OnUpdate(() =>
        {
            if (cardView != null && cardView.isHidden && mainCam != null)
            {
                var toCard = (mainCam.transform.position - card.position).normalized;
                if (Vector3.Dot(card.up, toCard) > 0)
                {
                    cardView.SetVisibility(true);
                }
            }
        });

        // 5. Cleanly await completion of the whole sequence
        await seq.AsyncWaitForCompletion();
    }

    public virtual async Task LayoutCardsTask(bool animated = true)
    {
        var overlap = entitySeparation;
        var width = (cards.Count - 1) * overlap;
        var xPos = -width / 2f;
        var duration = animated ? 0.25f : 0;

        List<Task> tweenTasks = new List<Task>();

        for (int i = 0; i < cards.Count; ++i)
        {
            var canvas = cards[i].GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = baseSortOrder + i;
            }

            // Calculate normalized position from center (-1 for leftmost, 0 for center, 1 for rightmost)
            float normalizedIndex = cards.Count > 1
                ? (2f * i / (cards.Count - 1)) - 1f
                : 0f;

            // Combine base handle position with calculated offsets
            Vector3 targetPosition = originPoint.position + new Vector3(xPos, 0, 0);
            Vector3 targetRotation = originPoint.rotation.eulerAngles + new Vector3(0, 0, 0);

            // Apply Tweens
            cards[i].DORotate(targetRotation, duration);
            Tweener moveTweener = cards[i].DOMove(targetPosition, duration);

            if (moveTweener != null && animated)
            {
                tweenTasks.Add(moveTweener.AsyncWaitForCompletion());
            }

            xPos += overlap;
        }

        if (tweenTasks.Count > 0)
        {
            await Task.WhenAll(tweenTasks);
        }
    }

    public virtual UICardEntity GetView(Entity card)
    {
        foreach (Transform t in cards)
        {
            var cardView = t.GetComponent<UICardEntity>();
            if (cardView != null && cardView.Entity == card)
            {
                return cardView;
            }
        }
        return null;
    }

    public virtual void Dismiss(UICardEntity card)
    {
        cards.Remove(card.transform);

        card.onDrag -= HandleDrag;
        card.onDrop -= HandleDrop;

        /* var poolable = card.GetComponent<Poolable>();
        var boardView = GetComponentInParent<BoardView>();
        if (boardView != null)
        {
            boardView.cardPooler.Enqueue(poolable);
        }*/
    }

    protected async Task OverdrawCardTask(Transform card)
    {
        var cardView = card.GetComponent<UICardEntity>();
        if (cardView != null)
        {
            Dismiss(cardView);
        }
    }
}
